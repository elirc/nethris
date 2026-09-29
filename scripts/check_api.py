"""Black-box regressions against real Kestrel/EF/SQLite; never opens an existing user DB."""
from concurrent.futures import ThreadPoolExecutor
from contextlib import closing
from datetime import date, timedelta
from pathlib import Path
import argparse
import json
import sqlite3
import sys
import traceback
import uuid
from learning_server import Server, PASSWORD


def run(server, passed):
    def check(name):
        passed.append(name)
        print('PASS ' + name, flush=True)
    def reject(method, path, body, token, status, **kwargs):
        server.ok(method, path, body, token, expected=status, **kwargs)
    def page(kind, token):
        return server.ok('GET', kind + '?limit=100', token=token)['items']
    def one(kind, row, token):
        return server.ok('GET', kind + '/' + row['id'], token=token)
    def save(kind, row, body, token):
        return server.ok('PUT', kind + '/' + row['id'], body, token, row=row)
    def employee_input(row):
        return {key: row[key] for key in ['employeeNumber', 'firstName', 'lastName', 'email', 'phone', 'location',
                'hireDate', 'status', 'ptoBalanceHours', 'departmentId', 'jobRoleId', 'managerId']}
    assert server.ok('GET', 'setup')['initialized'] is False
    admin = server.setup(); at = admin['token']
    assert admin['user']['role'] == 'Admin' and server.ok('GET', 'setup')['initialized'] is True
    reject('POST', 'setup', {'companyName': 'Replacement', 'firstName': 'No', 'lastName': 'No', 'email': 'no@example.test', 'password': PASSWORD}, None, 409)
    check('bootstrap is atomic, named roles are serialized, repeat setup is refused')
    hr = server.login('hr@example.test'); ht = hr['token']
    manager = server.login('manager@example.test'); mt = manager['token']
    employee = server.login('employee@example.test'); et = employee['token']
    colleague = server.login('colleague@example.test'); ct = colleague['token']
    reject('POST', 'auth/login', {'email': 'employee@example.test', 'password': 'Incorrect-password-2026'}, None, 401)
    reject('GET', 'employees', None, None, 401)
    check('invalid credentials and anonymous directory access are rejected')
    employees = page('employees', at); alex = next(e for e in employees if e['email'] == 'employee@example.test')
    sam = next(e for e in employees if e['email'] == 'colleague@example.test')
    maya = next(e for e in employees if e['email'] == 'manager@example.test')
    owner = next(e for e in employees if e['email'] == 'admin@example.test')
    assert one('employees', sam, et)['ptoBalanceHours'] is None
    assert one('employees', alex, et)['ptoBalanceHours'] == 120
    assert one('employees', alex, mt)['ptoBalanceHours'] == 120
    assert all(r['salaryBandMin'] is None for r in page('job-roles', et))
    reject('GET', 'accounts', None, ht, 403); reject('GET', 'audit-logs', None, et, 403)
    check('balance, salary, account and audit projections respect current roles')
    departments = page('departments', at); roles = page('job-roles', at)
    reject('POST', 'employees', {**employee_input(alex), 'accountRole': 'Admin'}, ht, 400)
    reject('POST', 'accounts', {'employeeId': alex['id'], 'role': 'Admin', 'password': PASSWORD}, ht, 403)
    reject('PUT', 'employees/' + owner['id'], employee_input(owner), ht, 403, row=owner)
    check('HR cannot grant Admin through employee fields or account routes, or edit Admin employee')
    reject('GET', 'employees?limit=101', None, at, 422)
    first = server.ok('GET', 'employees?limit=2', token=at); second = server.ok('GET', 'employees?limit=2&offset=2', token=at)
    assert first['total'] == 6 and len(first['items']) == 2 and not ({r['id'] for r in first['items']} & {r['id'] for r in second['items']})
    assert server.ok('GET', 'employees?search=Rivera', token=at)['total'] == 1
    assert server.ok('GET', 'org-chart', token=at)[0]['id'] == owner['id']
    check('directory filtering, SQL pages and bounded org traversal return real relationships')
    reject('POST', 'departments', {'name': 'Bad'}, at, 422, headers={'Idempotency-Key': 'not-a-uuid'})
    reject('POST', 'departments', {'name': 'Bad', 'unexpected': 1}, at, 400)
    reject('POST', 'departments', None, at, 400, raw=b'{invalid')
    reject('POST', 'departments', {'name': 'Bad'}, at, 415, headers={'Content-Type': 'text/plain'})
    reject('POST', 'departments', None, at, 413, raw=b' ' * 33000)
    reject('POST', 'departments', {'name': 'Bad'}, at, 403, headers={'Origin': 'https://untrusted.example'})
    reject('GET', 'health', None, None, 400, headers={'Host': 'untrusted.example'})
    check('HTTP enforces operation keys, JSON shape, content type, body size, origin and host')
    body = {'name': 'Workshop Lab', 'costCenter': 'LAB', 'leadEmployeeId': None, 'isActive': True}; key = str(uuid.uuid4())
    created = server.ok('POST', 'departments', body, at, expected=201, key=key)
    status, replay, headers = server.call('POST', 'departments', body, at, key=key)
    assert status == 201 and replay == created and headers.get('Idempotency-Replayed') == 'true'
    reject('POST', 'departments', {**body, 'name': 'Different'}, at, 409, key=key)
    reject('POST', 'departments', {**body, 'name': 'workshop lab'}, at, 409)
    check('creation receipts replay exactly and reject key reuse or case-insensitive duplicates')
    reject('PUT', 'departments/' + created['id'], body, at, 428)
    changed = save('departments', created, {**body, 'name': 'Updated Lab'}, at)
    assert changed['version'] == created['version'] + 1
    reject('PUT', 'departments/' + created['id'], body, at, 409, row=created)
    reject('PUT', 'departments/' + changed['id'], {**body, 'leadEmployeeId': alex['id']}, at, 422, row=changed)
    server.ok('DELETE', 'departments/' + changed['id'], token=at, row=changed)
    reject('GET', 'departments/' + changed['id'], None, at, 404)
    check('department CRUD requires versions and valid department leadership')
    engineering = next(d for d in departments if d['name'] == 'Engineering')
    reject('DELETE', 'departments/' + engineering['id'], None, at, 409, row=engineering)
    archived = save('departments', engineering, {'name': 'Engineering', 'costCenter': 'ENG-100', 'leadEmployeeId': maya['id'], 'isActive': False}, at)
    new_employee = {**employee_input(alex), 'employeeNumber': 'TEST-100', 'email': 'new@example.test', 'firstName': 'New', 'managerId': None}
    reject('POST', 'employees', new_employee, ht, 409)
    engineering = save('departments', archived, {'name': 'Engineering', 'costCenter': 'ENG-100', 'leadEmployeeId': maya['id'], 'isActive': True}, at)
    check('referenced departments are protected and archives block new assignments')
    role_body = {'title': 'Tester', 'level': 'IC1', 'salaryBandMin': 50000.25, 'salaryBandMax': 70000.5, 'isActive': True}
    job = server.ok('POST', 'job-roles', role_body, ht, expected=201)
    assert job['salaryBandMin'] == 50000.25
    reject('PUT', 'job-roles/' + job['id'], {**role_body, 'salaryBandMax': 40000}, ht, 422, row=job)
    reject('PUT', 'job-roles/' + job['id'], {**role_body, 'salaryBandMin': 1.005}, ht, 422, row=job)
    job = save('job-roles', job, {**role_body, 'level': 'IC2'}, ht)
    server.ok('DELETE', 'job-roles/' + job['id'], token=ht, row=job)
    check('job-role CRUD preserves exact cents and validates salary bands')
    person = server.ok('POST', 'employees', new_employee, ht, expected=201)
    reject('POST', 'employees', {**new_employee, 'employeeNumber': 'other'}, ht, 409)
    reject('PUT', 'employees/' + person['id'], {**new_employee, 'managerId': person['id']}, ht, 409, row=person)
    reject('PUT', 'employees/' + maya['id'], {**employee_input(maya), 'managerId': alex['id']}, at, 409, row=maya)
    person = save('employees', person, {**new_employee, 'firstName': 'Edited'}, ht)
    assert person['firstName'] == 'Edited'
    server.ok('DELETE', 'employees/' + person['id'], token=ht, row=person)
    reject('DELETE', 'employees/' + alex['id'], None, ht, 409, row=alex)
    check('employee CRUD rejects duplicate identity, cycles and referenced deletion')
    person = server.ok('POST', 'employees', new_employee, ht, expected=201)
    reject('POST', 'auth/login', {'email': new_employee['email'], 'password': PASSWORD}, None, 401)
    reject('POST', 'accounts', {'employeeId': person['id'], 'role': 'Employee', 'password': 'short'}, at, 422)
    account = server.ok('POST', 'accounts', {'employeeId': person['id'], 'role': 'Employee', 'password': PASSWORD}, at, expected=201)
    assert 'passwordHash' not in account
    reject('POST', 'accounts', {'employeeId': person['id'], 'role': 'Employee', 'password': PASSWORD}, at, 409)
    personal_token = server.login(new_employee['email'])['token']
    person = save('employees', person, {**new_employee, 'email': 'renamed@example.test'}, ht)
    reject('POST', 'auth/login', {'email': new_employee['email'], 'password': PASSWORD}, None, 401)
    assert server.login('renamed@example.test')['user']['employeeId'] == person['id']
    check('account provisioning is separate, unique, password-bounded and follows employee email edits')
    account = one('accounts', account, at)
    account = save('accounts', account, {'role': 'Employee', 'isActive': True, 'password': PASSWORD + '-new'}, at)
    reject('GET', 'me', None, personal_token, 401)
    reject('POST', 'auth/login', {'email': 'renamed@example.test', 'password': PASSWORD}, None, 401)
    assert server.ok('POST', 'auth/login', {'email': 'renamed@example.test', 'password': PASSWORD + '-new'})['user']['role'] == 'Employee'
    reject('PUT', 'employees/' + alex['id'], employee_input(alex), et, 403, row=alex)
    check('Admin password reset revokes existing sessions; ordinary employees cannot edit directory data')
    start = date.today() + timedelta(days=10)
    pto_body = {'startDate': str(start), 'endDate': str(start), 'hours': 8, 'reason': 'Workshop break'}
    reject('POST', 'pto', {**pto_body, 'hours': 9999}, et, 422)
    reject('POST', 'pto', {**pto_body, 'hours': .1}, et, 422)
    reject('POST', 'pto', {**pto_body, 'startDate': str(date.today() - timedelta(days=1))}, et, 422)
    reject('POST', 'pto', {**pto_body, 'endDate': str(start + timedelta(days=31))}, et, 422)
    check('PTO rejects original 9999-hour defect, fractional units, past and overlong dates')
    pto = server.ok('POST', 'pto', pto_body, et, expected=201)
    assert pto['status'] == 'PendingManager' and pto['assignedManagerId'] == maya['id']
    reject('POST', 'pto', pto_body, et, 409)
    reject('GET', 'pto/' + pto['id'], None, ct, 404)
    reject('PUT', 'employees/' + alex['id'], {**employee_input(alex), 'ptoBalanceHours': 4}, ht, 409, row=alex)
    check('PTO reserves balance, rejects overlap and hides another employee request')
    pto = save('pto', pto, {**pto_body, 'reason': 'Updated reason', 'hours': 7.75}, et)
    reject('POST', 'pto/' + pto['id'] + '/decision', {'decision': 'Approved', 'comment': None}, et, 403, row=pto)
    reject('POST', 'pto/' + pto['id'] + '/decision', {'decision': 'Approved', 'comment': None}, ht, 403, row=pto)
    reject('POST', 'pto/' + pto['id'] + '/decision', {'decision': 0, 'comment': None}, mt, 400, row=pto)
    manager_decision = {'decision': 'Approved', 'comment': 'Coverage arranged'}; manager_key = str(uuid.uuid4())
    reviewed = server.ok('POST', 'pto/' + pto['id'] + '/decision', manager_decision, mt, row=pto, key=manager_key)
    assert reviewed['status'] == 'PendingHr' and len(reviewed['approvals']) == 1
    reject('PUT', 'pto/' + reviewed['id'], pto_body, et, 409, row=reviewed)
    reject('POST', 'pto/' + reviewed['id'] + '/decision', {'decision': 'Approved', 'comment': None}, mt, 403, row=reviewed)
    check('pending edits work; self, skipped-stage, enum and repeated-review guards hold')
    approval_key = str(uuid.uuid4()); decision = {'decision': 'Approved', 'comment': 'Balance checked'}
    approved = server.ok('POST', 'pto/' + reviewed['id'] + '/decision', decision, ht, row=reviewed, key=approval_key)
    assert approved['status'] == 'Approved' and len(approved['approvals']) == 2
    assert one('employees', alex, at)['ptoBalanceHours'] == 112.25
    assert server.ok('POST', 'pto/' + reviewed['id'] + '/decision', decision, ht, row=reviewed, key=approval_key) == approved
    assert one('employees', alex, at)['ptoBalanceHours'] == 112.25
    reject('POST', 'pto/' + approved['id'] + '/cancel', {}, et, 409, row=approved)
    check('HR approval inserts history and deducts once; exact retry preserves balance')
    hr_pto = server.ok('POST', 'pto', pto_body, ht, expected=201)
    hr_pto = server.ok('POST', 'pto/' + hr_pto['id'] + '/decision', decision, at, row=hr_pto)
    reject('POST', 'pto/' + hr_pto['id'] + '/decision', decision, at, 403, row=hr_pto)
    reviewer_token = server.login('reviewer@example.test')['token']
    assert server.ok('POST', 'pto/' + hr_pto['id'] + '/decision', decision, reviewer_token, row=hr_pto)['status'] == 'Approved'
    owner_pto = server.ok('POST', 'pto', pto_body, at, expected=201)
    assert owner_pto['status'] == 'PendingHr'
    reject('POST', 'pto/' + owner_pto['id'] + '/decision', decision, at, 403, row=owner_pto)
    assert server.ok('POST', 'pto/' + owner_pto['id'] + '/decision', decision, ht, row=owner_pto)['status'] == 'Approved'
    check('HR stage requires a distinct reviewer; no-manager requests start at HR without self approval')
    second_body = {**pto_body, 'startDate': str(start + timedelta(days=2)), 'endDate': str(start + timedelta(days=2))}
    second_pto = server.ok('POST', 'pto', second_body, et, expected=201)
    second_pto = server.ok('POST', 'pto/' + second_pto['id'] + '/decision', {'decision': 'Approved', 'comment': None}, mt, row=second_pto)
    reject('DELETE', 'pto/' + second_pto['id'], None, et, 409, row=second_pto)
    cancelled = server.ok('POST', 'pto/' + second_pto['id'] + '/cancel', {}, et, row=second_pto)
    assert cancelled['status'] == 'Cancelled' and len(cancelled['approvals']) == 1
    second_pto = server.ok('POST', 'pto', second_body, et, expected=201)
    server.ok('DELETE', 'pto/' + second_pto['id'], token=et, row=second_pto)
    check('cancellation preserves decisions, releases dates, and undecided deletion works')
    long_body = {**pto_body, 'startDate': str(start + timedelta(days=20)), 'endDate': str(start + timedelta(days=40)), 'hours': 112.25}
    reserved = server.ok('POST', 'pto', long_body, et, expected=201)
    reject('POST', 'pto', second_body, et, 409)
    server.ok('DELETE', 'pto/' + reserved['id'], token=et, row=reserved)
    check('multiple pending requests cannot over-reserve remaining balance')
    concurrent_key = str(uuid.uuid4())
    with ThreadPoolExecutor(max_workers=2) as pool:
        results = list(pool.map(lambda _: server.call('POST', 'pto', second_body, et, key=concurrent_key), range(2)))
    assert [r[0] for r in results] == [201, 201] and results[0][1] == results[1][1]
    concurrent = results[0][1]
    with ThreadPoolExecutor(max_workers=2) as pool:
        decisions = list(pool.map(lambda _: server.call('POST', 'pto/' + concurrent['id'] + '/decision', {'decision': 'Approved', 'comment': None}, mt, row=concurrent), range(2)))
    assert sorted(r[0] for r in decisions) == [200, 409]
    check('concurrent create retries produce one request and competing decisions have one winner')
    audit = page('audit-logs', at)
    assert len(audit) > 10 and all('actorEmail' in item for item in audit)
    assert [a['createdAt'] for a in audit] == sorted([a['createdAt'] for a in audit], reverse=True)
    assert page('pto', at)
    check('PTO and audit ordering execute successfully in real SQLite HTTP reads')
    with closing(sqlite3.connect(server.database)) as db, db:
        old_count = db.execute('SELECT COUNT(*) FROM Departments').fetchone()[0]
        old_audits = db.execute('SELECT COUNT(*) FROM AuditLogs').fetchone()[0]
        db.execute("CREATE TRIGGER fail_receipt BEFORE INSERT ON Receipts BEGIN SELECT RAISE(ABORT, 'test-only receipt failure'); END")
    reject('POST', 'departments', {**body, 'name': 'Must roll back'}, at, 409)
    with closing(sqlite3.connect(server.database)) as db, db:
        assert db.execute('SELECT COUNT(*) FROM Departments').fetchone()[0] == old_count
        assert db.execute('SELECT COUNT(*) FROM AuditLogs').fetchone()[0] == old_audits
        db.execute('DROP TRIGGER fail_receipt')
    check('late receipt failure rolls back earlier domain SaveChanges and audit writes')
    accounts = page('accounts', at); owner_account = next(a for a in accounts if a['role'] == 'Admin')
    reject('PUT', 'accounts/' + owner_account['id'], {'role': 'Employee', 'isActive': True, 'password': None}, at, 409, row=owner_account)
    reject('PUT', 'accounts/' + owner_account['id'], {'role': 'Admin', 'isActive': False, 'password': None}, at, 409, row=owner_account)
    check('last active Admin cannot be demoted or deactivated')
    sam_account = next(a for a in accounts if a['employeeId'] == sam['id'])
    promoted = save('accounts', sam_account, {'role': 'Hr', 'isActive': True, 'password': None}, at)
    assert server.ok('GET', 'me', token=ct)['role'] == 'Hr'
    save('accounts', promoted, {'role': 'Hr', 'isActive': False, 'password': None}, at)
    reject('GET', 'me', None, ct, 401)
    check('existing JWT observes current role and account deactivation immediately')
    replay_key = str(uuid.uuid4())
    server.ok('POST', 'departments', {**body, 'name': 'HR receipt'}, ht, expected=201, key=replay_key)
    hr_account = next(a for a in accounts if a['employeeId'] == hr['user']['employeeId'])
    save('accounts', hr_account, {'role': 'Employee', 'isActive': True, 'password': None}, at)
    reject('POST', 'departments', {**body, 'name': 'HR receipt'}, ht, 403, key=replay_key)
    reject('POST', 'pto/' + reviewed['id'] + '/decision', decision, ht, 403, row=reviewed, key=approval_key)
    check('authorization is rechecked before replaying a receipt')
    current_alex = one('employees', alex, at)
    save('employees', current_alex, {**employee_input(current_alex), 'managerId': owner['id']}, at)
    reject('GET', 'pto/' + approved['id'], None, mt, 404)
    reject('POST', 'pto/' + pto['id'] + '/decision', manager_decision, mt, 403, row=pto, key=manager_key)
    check('manager reassignment removes old request visibility and blocks old decision replay')
    server.ok('POST', 'auth/logout', {}, mt)
    reject('GET', 'me', None, mt, 401)
    check('logout revokes the server session')
    assert server.ok('GET', 'health')['status'] == 'ok'


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--report', type=Path, default=Path('.learning-results/api.json'))
    args = parser.parse_args(); passed = []; error = None
    try:
        with Server() as server:
            run(server, passed)
    except BaseException:
        error = traceback.format_exc()
        print(error, file=sys.stderr)
    result = {'passed': error is None, 'count': len(passed), 'checks': passed, 'error': error,
              'database': 'fresh temporary test database; removed with owned server stopped'}
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(result, indent=2), encoding='utf-8')
    return 0 if error is None else 1


if __name__ == '__main__':
    sys.exit(main())
