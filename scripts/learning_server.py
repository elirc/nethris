"""Private test server: a fresh temporary database, loopback port, and owned process."""
from pathlib import Path
import json
import os
import socket
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.request
import uuid

ROOT = Path(__file__).resolve().parents[1]
PASSWORD = 'Workshop-Tests-Only-2026!'


def tag(row):
    return '"' + row['id'].replace('-', '') + '-' + str(row['version']) + '"'


class Server:
    def __init__(self):
        self.temp = tempfile.TemporaryDirectory(prefix='nethris-check-')
        self.directory = Path(self.temp.name)
        self.database = self.directory / 'test.sqlite'
        self.process = None
        self.log = None

    def __enter__(self):
        assembly = Path(os.environ.get('NETHRIS_ASSEMBLY', ROOT / 'artifacts/Nethris.Api.dll')).resolve()
        if not assembly.is_file():
            self.temp.cleanup()
            raise RuntimeError('Build the package with scripts/package.ps1 first, or set NETHRIS_ASSEMBLY.')
        with socket.socket() as probe:
            probe.bind(('127.0.0.1', 0))
            port = probe.getsockname()[1]
        self.base = f'http://127.0.0.1:{port}'
        env = dict(os.environ, Hris__Database=str(self.database), Hris__Urls=self.base,
                   ASPNETCORE_ENVIRONMENT='Production', DOTNET_NOLOGO='1')
        self.log = (self.directory / 'server.log').open('w', encoding='utf-8')
        self.process = subprocess.Popen(['dotnet', str(assembly)], cwd=assembly.parent,
                                        env=env, stdout=self.log, stderr=subprocess.STDOUT)
        try:
            deadline = time.monotonic() + 90
            while time.monotonic() < deadline:
                if self.process.poll() is not None:
                    raise RuntimeError('Owned test server exited during startup.')
                try:
                    if self.call('GET', 'health')[0] == 200:
                        return self
                except (OSError, urllib.error.URLError):
                    pass
                time.sleep(.2)
            raise RuntimeError('Owned test server did not become ready.')
        except BaseException:
            self.__exit__(None, None, None)
            raise

    def __exit__(self, error_type, *_):
        if self.process is not None and self.process.poll() is None:
            self.process.terminate()
            try:
                self.process.wait(timeout=15)
            except subprocess.TimeoutExpired:
                self.process.kill()
                self.process.wait(timeout=15)
        if self.log is not None:
            self.log.close()
        if error_type is not None and (self.directory / 'server.log').exists():
            print((self.directory / 'server.log').read_text(encoding='utf-8', errors='replace')[-16000:], file=sys.stderr)
        self.temp.cleanup()

    def call(self, method, path, body=None, token=None, key=None, row=None, headers=None, raw=None):
        request_headers = {'Content-Type': 'application/json'}
        if token:
            request_headers['Authorization'] = 'Bearer ' + token
        if method != 'GET':
            request_headers['Idempotency-Key'] = key or str(uuid.uuid4())
        if row:
            request_headers['If-Match'] = tag(row)
        request_headers.update(headers or {})
        data = raw if raw is not None else json.dumps(body).encode() if body is not None else None
        request = urllib.request.Request(self.base + '/api/' + path, data=data, method=method, headers=request_headers)
        try:
            response = urllib.request.urlopen(request, timeout=30)
        except urllib.error.HTTPError as error:
            response = error
        with response:
            text = response.read().decode('utf-8')
            try:
                value = json.loads(text)
            except ValueError:
                value = {'unparsed': text[:300]}
            return response.status, value, dict(response.headers)

    def ok(self, method, path, body=None, token=None, expected=200, **kwargs):
        status, data, headers = self.call(method, path, body, token, **kwargs)
        assert status == expected, (method, path, status, expected, data)
        return data

    def login(self, email):
        return self.ok('POST', 'auth/login', {'email': email, 'password': PASSWORD})

    def setup(self):
        return self.ok('POST', 'setup', {'companyName': 'Nethris test workshop', 'firstName': 'Avery',
                       'lastName': 'Admin', 'email': 'admin@example.test', 'password': PASSWORD, 'seedDemo': True}, expected=201)
