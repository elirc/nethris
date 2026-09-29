"""Real browser acceptance checks against a private packaged HRIS server."""
from pathlib import Path
from datetime import date, timedelta
import argparse
import json
import os
import sys
import time
import traceback
from playwright.sync_api import sync_playwright, expect
from learning_server import Server, PASSWORD


def run(server, browser, screenshots, passed):
    errors = []
    def context():
        result = browser.new_context(viewport={'width': 1440, 'height': 1000})
        result.on('page', lambda page: page.on('pageerror', lambda error: errors.append(str(error))))
        return result
    def check(name):
        passed.append(name); print('PASS ' + name, flush=True)
    def nav(page, name):
        page.get_by_role('navigation', name='Workspace').get_by_role('button', name=name, exact=True).click()
        expect(page.get_by_role('heading', name=name, exact=True).first).to_be_visible()
    def record(page, name):
        return page.locator('article.record').filter(has=page.get_by_role('heading', name=name, exact=True))
    def save(page):
        page.get_by_role('button', name='Save changes', exact=True).click()
        expect(page.get_by_role('status')).to_contain_text('Saved successfully')
        expect(page.locator('.editor')).to_have_count(0)
    def login(email):
        ctx = context(); page = ctx.new_page(); page.goto(server.base)
        expect(page.get_by_role('heading', name='Welcome back')).to_be_visible()
        page.get_by_label('Email', exact=True).fill(email); page.get_by_label('Password', exact=True).fill(PASSWORD)
        page.get_by_role('button', name='Sign in', exact=True).click()
        expect(page.get_by_role('heading', name='People directory', exact=True).first).to_be_visible()
        expect(page.locator('article.record')).to_have_count(6)
        return ctx, page
    admin_context = context(); admin = admin_context.new_page(); admin.goto(server.base)
    expect(admin.get_by_role('heading', name='Create your workshop')).to_be_visible()
    admin.get_by_label('Company name', exact=True).fill('Northstar Studio')
    admin.get_by_label('First name', exact=True).fill('Avery'); admin.get_by_label('Last name', exact=True).fill('Admin')
    admin.get_by_label('Email', exact=True).fill('admin@example.test'); admin.get_by_label('Password', exact=True).fill(PASSWORD)
    admin.get_by_role('button', name='Create workshop', exact=True).click()
    expect(admin.get_by_role('heading', name='People directory', exact=True).first).to_be_visible()
    expect(admin.locator('article.record')).to_have_count(6)
    check('workshop bootstrap through labeled forms creates a real directory')
    invalid_context = context(); invalid = invalid_context.new_page(); invalid.goto(server.base)
    invalid.get_by_label('Email', exact=True).fill('employee@example.test'); invalid.get_by_label('Password', exact=True).fill('Wrong-password-2026')
    invalid.get_by_role('button', name='Sign in', exact=True).click()
    expect(invalid.get_by_role('alert')).to_contain_text('incorrect')
    expect(invalid.get_by_role('navigation', name='Workspace')).to_have_count(0)
    invalid_context.close(); check('rejected API login stays rejected without demo fallback')
    nav(admin, 'Departments'); admin.get_by_role('button', name='+ Add record', exact=True).click()
    admin.get_by_label('Department name', exact=True).fill('Learning Lab')
    admin.get_by_label('Cost center', exact=False).fill('LAB-300'); save(admin)
    lab = record(admin, 'Learning Lab'); expect(lab).to_be_visible()
    lab.get_by_role('button', name='Edit', exact=True).click()
    admin.get_by_label('Department name', exact=True).fill('Draft Lab')
    api_admin = server.login('admin@example.test')['token']
    saved_lab = next(d for d in server.ok('GET', 'departments?limit=100', token=api_admin)['items'] if d['name'] == 'Learning Lab')
    server.ok('PUT', 'departments/' + saved_lab['id'], {'name': 'Colleague Lab', 'costCenter': 'LAB-400', 'leadEmployeeId': None, 'isActive': True}, api_admin, row=saved_lab)
    admin.get_by_role('button', name='Save changes', exact=True).click()
    expect(admin.get_by_role('heading', name='Review the latest saved record')).to_be_visible()
    expect(admin.get_by_label('Department name', exact=True)).to_have_value('Draft Lab')
    expect(admin.locator('.comparison')).to_contain_text('Colleague Lab')
    admin.get_by_role('button', name='Use latest version, keep draft', exact=True).click(); save(admin)
    expect(record(admin, 'Draft Lab')).to_be_visible()
    check('stale edit shows current values and preserves the draft for explicit review')
    record(admin, 'Draft Lab').get_by_role('button', name='Delete', exact=True).click()
    admin.get_by_role('button', name='Confirm deletion', exact=True).click()
    expect(admin.locator('.editor')).to_have_count(0); expect(record(admin, 'Draft Lab')).to_have_count(0)
    check('department deletion uses an explicit confirmation and refreshes saved records')
    intercepted = []
    def lose_response(route):
        if route.request.method != 'POST':
            route.continue_(); return
        intercepted.append((route.request.headers['idempotency-key'], route.request.post_data))
        if len(intercepted) == 1:
            committed = route.fetch()
            assert committed.status == 201
            route.abort('failed')
        else:
            route.continue_()
    admin.route('**/api/departments', lose_response)
    admin.get_by_role('button', name='+ Add record', exact=True).click()
    admin.get_by_label('Department name', exact=True).fill('Recovered Department')
    admin.get_by_role('button', name='Save changes', exact=True).click()
    expect(admin.get_by_role('button', name='Retry exact operation')).to_be_visible()
    expect(admin.get_by_label('Department name', exact=True)).to_be_disabled()
    admin.get_by_role('button', name='Retry exact operation').click()
    expect(admin.locator('.editor')).to_have_count(0); expect(record(admin, 'Recovered Department')).to_have_count(1)
    assert len(intercepted) == 2 and intercepted[0] == intercepted[1]
    assert len([d for d in server.ok('GET', 'departments?limit=100', token=api_admin)['items'] if d['name'] == 'Recovered Department']) == 1
    admin.unroute('**/api/departments', lose_response)
    check('lost response after commit recovers through an exact browser retry without duplication')
    nav(admin, 'Job roles'); admin.get_by_role('button', name='+ Add record', exact=True).click()
    admin.get_by_label('Job title', exact=True).fill('Learning engineer'); admin.get_by_label('Level', exact=True).fill('IC1')
    admin.get_by_label('Salary band minimum', exact=True).fill('50000.25'); admin.get_by_label('Salary band maximum', exact=True).fill('75000.50'); save(admin)
    expect(record(admin, 'Learning engineer')).to_contain_text('50000.25')
    record(admin, 'Learning engineer').get_by_role('button', name='Edit', exact=True).click()
    admin.get_by_label('Level', exact=True).fill('IC2'); save(admin)
    expect(record(admin, 'Learning engineer')).to_contain_text('IC2')
    record(admin, 'Learning engineer').get_by_role('button', name='Delete', exact=True).click(); admin.get_by_role('button', name='Confirm deletion').click()
    expect(record(admin, 'Learning engineer')).to_have_count(0)
    check('job-role forms create, edit and delete exact salary bands')
    nav(admin, 'People directory'); admin.get_by_role('button', name='+ Add record', exact=True).click()
    for label, text in [('Employee number', 'LAB-001'), ('First name', 'Taylor'), ('Last name', 'Learner'), ('Email', 'taylor@example.test'), ('Location', 'Remote'), ('PTO balance hours', '40.25')]:
        admin.get_by_label(label, exact=label != 'Location').fill(text)
    save(admin); expect(record(admin, 'Taylor Learner')).to_be_visible()
    record(admin, 'Taylor Learner').get_by_role('button', name='Edit', exact=True).click()
    admin.get_by_label('First name', exact=True).fill('Tay'); save(admin)
    expect(record(admin, 'Tay Learner')).to_be_visible()
    record(admin, 'Tay Learner').get_by_role('button', name='Delete', exact=True).click(); admin.get_by_role('button', name='Confirm deletion').click()
    expect(record(admin, 'Tay Learner')).to_have_count(0)
    check('employee editor performs real create, edit and unreferenced delete')
    nav(admin, 'Account access')
    record(admin, 'Sam Okafor').get_by_role('button', name='Edit', exact=True).click()
    admin.get_by_label('Account role', exact=True).select_option('Manager'); save(admin)
    expect(record(admin, 'Sam Okafor')).to_contain_text('Manager')
    record(admin, 'Sam Okafor').get_by_role('button', name='Edit', exact=True).click()
    admin.get_by_label('Account role', exact=True).select_option('Employee'); save(admin)
    expect(record(admin, 'Sam Okafor')).to_contain_text('Employee')
    check('Admin account editor changes and restores a real account role')
    record(admin, 'Avery Admin').get_by_role('button', name='Edit', exact=True).click()
    admin.get_by_label('New password (leave blank to keep)', exact=False).fill(PASSWORD)
    admin.get_by_role('button', name='Save changes', exact=True).click()
    expect(admin.get_by_role('heading', name='Welcome back')).to_be_visible()
    expect(admin.get_by_role('status')).to_contain_text('The change was saved')
    admin.get_by_label('Email', exact=True).fill('admin@example.test'); admin.get_by_label('Password', exact=True).fill(PASSWORD)
    admin.get_by_role('button', name='Sign in', exact=True).click()
    expect(admin.get_by_role('heading', name='Account access', exact=True).first).to_be_visible()
    check('self password reset returns to sign-in after the server revokes the session')
    nav(admin, 'Organization'); expect(admin.locator('.org-person')).to_have_count(6)
    expect(admin.locator('.org-tree')).to_contain_text('Alex Rivera')
    check('organization view renders saved reporting relationships')
    employee_context, employee = login('employee@example.test')
    expect(employee.get_by_role('button', name='+ Add record', exact=True)).to_have_count(0)
    expect(employee.get_by_role('navigation').get_by_role('button', name='Account access')).to_have_count(0)
    expect(record(employee, 'Sam Okafor')).to_contain_text('Restricted')
    nav(employee, 'Job roles'); expect(record(employee, 'Software engineer')).to_contain_text('Restricted')
    check('employee UI hides management controls and private colleague values')
    nav(employee, 'Time off'); employee.get_by_role('button', name='+ Request time off', exact=True).click()
    start = str(date.today() + timedelta(days=10))
    employee.get_by_label('Start date', exact=True).fill(start); employee.get_by_label('End date', exact=True).fill(start)
    employee.get_by_label('Hours', exact=True).fill('7.75'); employee.get_by_label('Reason', exact=True).fill('Learning workshop break'); save(employee)
    expect(record(employee, 'Alex Rivera')).to_contain_text('PendingManager')
    record(employee, 'Alex Rivera').get_by_role('button', name='Edit', exact=True).click()
    employee.get_by_label('Reason', exact=True).fill('Updated learning break'); save(employee)
    expect(record(employee, 'Alex Rivera')).to_contain_text('Updated learning break')
    manager_context, manager = login('manager@example.test'); nav(manager, 'Time off')
    record(manager, 'Alex Rivera').get_by_role('button', name='Review request').click()
    manager.get_by_label('Review comment', exact=False).fill('Team coverage arranged')
    manager.get_by_role('button', name='Submit decision').click()
    expect(record(manager, 'Alex Rivera')).to_contain_text('PendingHr')
    hr_context, hr = login('hr@example.test'); nav(hr, 'Time off')
    record(hr, 'Alex Rivera').get_by_role('button', name='Review request').click()
    hr.get_by_label('Review comment', exact=False).fill('Approved against current balance')
    hr.get_by_role('button', name='Submit decision').click()
    expect(record(hr, 'Alex Rivera')).to_contain_text('Approved')
    expect(record(hr, 'Alex Rivera').locator('.approval-history li')).to_have_count(2)
    employee.get_by_role('button', name='Refresh', exact=True).click()
    expect(record(employee, 'Alex Rivera')).to_contain_text('Approved')
    nav(employee, 'People directory'); expect(record(employee, 'Alex Rivera')).to_contain_text('112.25')
    check('employee edit, manager review and HR finalization update real history and balance')
    nav(employee, 'Time off'); employee.get_by_role('button', name='+ Request time off', exact=True).click()
    other = str(date.today() + timedelta(days=12))
    employee.get_by_label('Start date', exact=True).fill(other); employee.get_by_label('End date', exact=True).fill(other)
    employee.get_by_label('Reason', exact=True).fill('Cancel this practice request'); save(employee)
    pending = employee.locator('article.record').filter(has_text='Cancel this practice request')
    pending.get_by_role('button', name='Cancel request').click(); employee.get_by_role('button', name='Confirm cancellation').click()
    expect(pending).to_contain_text('Cancelled')
    check('requester cancellation is visible and preserves the request record')
    nav(admin, 'Activity'); expect(admin.locator('article.record').first).to_contain_text('pto')
    nav(admin, 'People directory'); expect(admin.locator('article.record')).to_have_count(6)
    screenshots.mkdir(parents=True, exist_ok=True)
    admin.screenshot(path=str(screenshots / 'desktop.png'), full_page=True)
    admin.set_viewport_size({'width': 390, 'height': 844})
    assert admin.evaluate('document.documentElement.scrollWidth <= window.innerWidth')
    admin.screenshot(path=str(screenshots / 'narrow.png'), full_page=True)
    check('desktop and narrow layouts fit the viewport and show real records')
    held = []
    def delay_directory_response(route):
        if held:
            route.continue_(); return
        response = route.fetch()
        assert response.status == 200
        held.append((route, response))
    employee.route('**/api/employees?*', delay_directory_response)
    employee.get_by_role('button', name='Refresh', exact=True).click()
    deadline = time.monotonic() + 30
    while not held and time.monotonic() < deadline:
        employee.wait_for_timeout(100)
    assert len(held) == 1, 'Expected one completed server response held before browser delivery.'
    employee.get_by_role('button', name='Sign out', exact=True).click()
    expect(employee.get_by_role('heading', name='Welcome back')).to_be_visible()
    expect(employee.get_by_role('status')).to_contain_text('server session was revoked')
    check('browser sign-out waits for server revocation')
    held[0][0].fulfill(response=held[0][1])
    employee.wait_for_load_state('networkidle')
    expect(employee.get_by_role('heading', name='Welcome back')).to_be_visible()
    expect(employee.get_by_role('navigation', name='Workspace')).to_have_count(0)
    expect(employee.get_by_role('status')).to_contain_text('server session was revoked')
    employee.unroute('**/api/employees?*', delay_directory_response)
    check('a delayed refresh cannot restore a signed-out session or cached private views')
    assert not errors, errors
    check('acceptance workflows produce no uncaught browser errors')
    for ctx in [admin_context, employee_context, manager_context, hr_context]:
        ctx.close()


def main():
    parser = argparse.ArgumentParser(); parser.add_argument('--report', type=Path, default=Path('.learning-results/browser.json'))
    parser.add_argument('--screenshots', type=Path, default=Path('.learning-results/images'))
    args = parser.parse_args(); passed = []; error = None
    try:
        with Server() as server, sync_playwright() as playwright:
            kwargs = {'headless': True}
            if os.environ.get('ASTRA_BROWSER_EXECUTABLE'):
                kwargs['executable_path'] = os.environ['ASTRA_BROWSER_EXECUTABLE']
            browser = playwright.chromium.launch(**kwargs)
            try:
                expect.set_options(timeout=60000)
                run(server, browser, args.screenshots, passed)
            finally:
                browser.close()
    except BaseException:
        error = traceback.format_exc(); print(error, file=sys.stderr)
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps({'passed': error is None, 'count': len(passed), 'checks': passed, 'error': error}, indent=2), encoding='utf-8')
    return 0 if error is None else 1


if __name__ == '__main__':
    sys.exit(main())
