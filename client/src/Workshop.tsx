import { FormEvent, useEffect, useRef, useState } from 'react';
import { Building2, CalendarCheck, ClipboardList, LogOut, ShieldCheck, UsersRound } from 'lucide-react';
import { ApiError, allRows, operation, Operation, Page, request, Row, Session, User } from './workshopApi';
import './workshop.css';

type Entity = 'employees' | 'departments' | 'job-roles' | 'pto' | 'accounts';
type Tab = Entity | 'org-chart' | 'audit-logs';
type Draft = Record<string, string | boolean>;
type Editor = { kind: Entity; action: 'save' | 'delete' | 'cancel' | 'decision'; row?: Row; draft: Draft };
type Field = { name: string; label: string; type?: string; options?: [string, string][]; optional?: boolean; step?: string; min?: string; max?: string };
type Org = { id: string; name: string; title: string; department: string; reports: Org[] };
const labels: Record<Tab, string> = { employees: 'People directory', departments: 'Departments', 'job-roles': 'Job roles', pto: 'Time off', accounts: 'Account access', 'org-chart': 'Organization', 'audit-logs': 'Activity' };
const value = (row: Row, key: string) => row[key] == null ? '' : String(row[key]);
const title = (row: Row) => value(row, 'fullName') || value(row, 'name') || value(row, 'title') || value(row, 'employeeName') || value(row, 'email');
const today = () => new Date().toISOString().slice(0, 10);
const rank = (role: string) => ['Employee', 'Manager', 'Hr', 'Admin'].indexOf(role);
const empty: Page = { items: [], total: 0, offset: 0, limit: 12 };

export default function Workshop() {
  const [initialized, setInitialized] = useState<boolean>(); const [company, setCompany] = useState('Nethris');
  const [session, setSession] = useState<Session>(); const [tab, setTab] = useState<Tab>('employees');
  const [page, setPage] = useState<Page>(empty); const [offset, setOffset] = useState(0); const [search, setSearch] = useState(''); const [filter, setFilter] = useState('');
  const [references, setReferences] = useState<Record<string, Row[]>>({}); const [org, setOrg] = useState<Org[]>([]);
  const [editor, setEditor] = useState<Editor>(); const [latest, setLatest] = useState<Row>(); const [stale, setStale] = useState(false);
  const [pending, setPending] = useState<Operation>(); const [busy, setBusy] = useState(false); const busyRef = useRef(false);
  const [error, setError] = useState(''); const [notice, setNotice] = useState(''); const loadId = useRef(0);
  const canHr = !!session && rank(session.user.role) >= 2; const admin = session?.user.role === 'Admin';
  async function setupStatus() {
    try { const status = await request<{ initialized: boolean; companyName: string }>('setup'); setInitialized(status.initialized); setCompany(status.initialized ? status.companyName : 'Nethris'); setError(''); }
    catch (e) { setError(message(e)); }
  }
  useEffect(() => { void setupStatus(); }, []);
  function message(e: unknown) { return e instanceof Error ? e.message : 'The operation failed.'; }
  function clearWorkspace() {
    // Invalidate old reads before clearing cached views. A late response must not restore a signed-out session.
    ++loadId.current;
    setPage(empty); setReferences({}); setOrg([]); setEditor(undefined); setPending(undefined); setLatest(undefined); setStale(false);
  }
  function forgetSession() { clearWorkspace(); setSession(undefined); }
  async function refresh(active = session, selected = tab, start = offset, term = filter) {
    if (!active) return false;
    const generation = ++loadId.current;
    try {
      const current = await request<User>('me', active.token);
      if (generation !== loadId.current) return false;
      const allowed = selected === 'accounts' ? current.role === 'Admin' : selected === 'audit-logs' ? rank(current.role) >= 2 : true;
      if (!allowed) { setSession({ ...active, user: current }); setEditor(undefined); setTab('employees'); setOffset(0); return false; }
      const [data, departments, jobs, employees] = await Promise.all([
        request<Page | Org[]>(`${selected}${selected === 'org-chart' ? '' : `?offset=${start}&limit=12&search=${encodeURIComponent(term)}`}`, active.token),
        allRows('departments', active.token, 200), allRows('job-roles', active.token, 200), allRows('employees', active.token, 2000)
      ]);
      if (generation !== loadId.current) return false;
      setSession({ ...active, user: current }); setReferences({ departments, 'job-roles': jobs, employees });
      if (selected === 'org-chart') setOrg(data as Org[]); else setPage(data as Page);
      return true;
    } catch (e) { if (generation !== loadId.current) return false; throw e; }
  }
  useEffect(() => {
    if (!session) return;
    setError(''); setPage(empty);
    void refresh().catch(e => { setError(message(e)); if (e instanceof ApiError && e.status === 401) forgetSession(); });
    // Session identity is stable during a read; updates to current role do not trigger a loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [session?.token, tab, offset, filter]);
  async function signIn(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); if (busyRef.current) return;
    const form = new FormData(event.currentTarget); busyRef.current = true; setBusy(true); setError('');
    try {
      const data = Object.fromEntries(form.entries());
      const input = initialized ? { email: data.email, password: data.password } : { ...data, seedDemo: data.seedDemo === 'on' };
      const next = await request<Session>(initialized ? 'auth/login' : 'setup', undefined, operation(initialized ? 'auth/login' : 'setup', 'POST', input));
      clearWorkspace(); setSession(next); setInitialized(true); if (!initialized) setCompany(String(data.companyName)); setNotice('Signed in. Changes are saved to your local workshop database.');
    } catch (e) { setError(message(e)); await setupStatusAfterFailure(); }
    finally { busyRef.current = false; setBusy(false); }
  }
  async function setupStatusAfterFailure() {
    try { const status = await request<{ initialized: boolean; companyName: string }>('setup'); setInitialized(status.initialized); if (status.initialized) setCompany(status.companyName); }
    catch { /* Keep the original error and entered form. */ }
  }
  async function signOut() {
    if (!session || busyRef.current) return; busyRef.current = true; setBusy(true); setError('');
    try { await request('auth/logout', session.token, operation('auth/logout', 'POST', {})); forgetSession(); setNotice('Signed out. The server session was revoked.'); }
    catch (e) {
      if (e instanceof ApiError && e.status === 401) { forgetSession(); setNotice('The server session is no longer active. Sign in again.'); }
      else setError(message(e));
    }
    finally { busyRef.current = false; setBusy(false); }
  }
  function fields(kind: Entity, row?: Row): Field[] {
    const opts = (key: string, allow?: (r: Row) => boolean): [string, string][] => (references[key] ?? []).filter(r => !allow || allow(r)).map(r => [r.id, title(r) + (key === 'job-roles' ? ` · ${value(r, 'level')}` : '')]);
    if (kind === 'departments') return [{ name: 'name', label: 'Department name' }, { name: 'costCenter', label: 'Cost center', optional: true },
      { name: 'leadEmployeeId', label: 'Department lead', options: opts('employees', r => r.departmentId === row?.id && r.status !== 'Terminated'), optional: true }, { name: 'isActive', label: 'Active for new assignments', type: 'checkbox' }];
    if (kind === 'job-roles') return [{ name: 'title', label: 'Job title' }, { name: 'level', label: 'Level' }, { name: 'salaryBandMin', label: 'Salary band minimum', type: 'number', step: '0.01', min: '0', max: '100000000' }, { name: 'salaryBandMax', label: 'Salary band maximum', type: 'number', step: '0.01', min: '0', max: '100000000' }, { name: 'isActive', label: 'Active for new assignments', type: 'checkbox' }];
    if (kind === 'accounts') return [...(!row ? [{ name: 'employeeId', label: 'Employee', options: opts('employees', r => r.status !== 'Terminated') }] : []),
      { name: 'role', label: 'Account role', options: ['Employee', 'Manager', 'Hr', 'Admin'].map(r => [r, r] as [string, string]) },
      ...(row ? [{ name: 'isActive', label: 'Account enabled', type: 'checkbox' }] : []), { name: 'password', label: row ? 'New password (leave blank to keep)' : 'Initial password', type: 'password', optional: !!row }];
    if (kind === 'pto') return [{ name: 'startDate', label: 'Start date', type: 'date', min: today() }, { name: 'endDate', label: 'End date', type: 'date', min: today() }, { name: 'hours', label: 'Hours', type: 'number', step: '0.25', min: '0.25', max: '240' }, { name: 'reason', label: 'Reason', type: 'textarea' }];
    return [{ name: 'employeeNumber', label: 'Employee number' }, { name: 'firstName', label: 'First name' }, { name: 'lastName', label: 'Last name' }, { name: 'email', label: 'Email', type: 'email' }, { name: 'phone', label: 'Phone', optional: true }, { name: 'location', label: 'Location', optional: true }, { name: 'hireDate', label: 'Hire date', type: 'date' },
      { name: 'status', label: 'Employment status', options: ['Active', 'OnLeave', 'Terminated'].map(s => [s, s]) }, { name: 'ptoBalanceHours', label: 'PTO balance hours', type: 'number', step: '0.25', min: '0', max: '10000' },
      { name: 'departmentId', label: 'Department', options: opts('departments', r => r.isActive === true || r.id === row?.departmentId) },
      { name: 'jobRoleId', label: 'Job role', options: opts('job-roles', r => r.isActive === true || r.id === row?.jobRoleId) }, { name: 'managerId', label: 'Manager', options: opts('employees', r => r.id !== row?.id && r.status !== 'Terminated'), optional: true }];
  }
  function open(kind: Entity, row?: Row, action: Editor['action'] = 'save') {
    const draft: Draft = {};
    for (const field of fields(kind, row)) draft[field.name] = row?.[field.name] == null ? (field.type === 'checkbox' ? true : field.type === 'number' ? (field.name === 'hours' ? '8' : '0') : field.type === 'date' ? today() : field.options && !field.optional ? field.options[0]?.[0] ?? '' : '') : field.type === 'checkbox' ? Boolean(row[field.name]) : String(row[field.name]);
    if (kind === 'accounts') draft.password = '';
    if (action === 'decision') { draft.decision = 'Approved'; draft.comment = ''; }
    setEditor({ kind, row, action, draft }); setLatest(undefined); setStale(false); setError(''); setNotice('');
  }
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault(); if (!editor || busyRef.current || pending || stale) return;
    let data: Record<string, unknown> = {};
    for (const field of fields(editor.kind, editor.row)) {
      const raw = editor.draft[field.name]; data[field.name] = field.type === 'number' ? Number(raw) : field.type === 'checkbox' ? Boolean(raw) : raw === '' && field.optional ? null : raw;
    }
    if (editor.action === 'decision') data = { decision: editor.draft.decision, comment: editor.draft.comment || null };
    if (editor.action === 'cancel') data = {};
    const path = `${editor.kind}${editor.row ? `/${editor.row.id}` : ''}${editor.action === 'decision' || editor.action === 'cancel' ? `/${editor.action}` : ''}`;
    const method = editor.action === 'delete' ? 'DELETE' : editor.action === 'save' && editor.row ? 'PUT' : 'POST';
    await perform(operation(path, method, method === 'DELETE' ? undefined : data, editor.row));
  }
  async function perform(op: Operation) {
    if (!session || busyRef.current) return; busyRef.current = true; setBusy(true); setError(''); setNotice(''); setPending(op);
    try {
      await request(op.path, session.token, op); setPending(undefined); setEditor(undefined); setStale(false); setNotice('Saved successfully.');
      try { await refresh(); } catch (e) {
        if (e instanceof ApiError && e.status === 401) { forgetSession(); setNotice('The change was saved. Sign in again because the session is no longer active.'); }
        else setError(`The change was saved, but refresh failed: ${message(e)}`);
      }
    } catch (e) {
      setError(message(e));
      if (e instanceof ApiError && e.status !== 0 && e.status < 500) {
        setPending(undefined);
        if (e.status === 409 && editor?.row) {
          setStale(true); setLatest(undefined);
          try {
            const saved = await request<Row>(`${editor.kind}/${editor.row.id}`, session.token);
            setLatest(saved); setStale(saved.version !== editor.row.version);
          } catch { /* Deleted or access changed; close and refresh. */ }
        }
        if (e.status === 401) forgetSession();
      }
    } finally { busyRef.current = false; setBusy(false); }
  }
  function fieldInput(field: Field) {
    if (!editor) return null;
    const update = (next: string | boolean) => setEditor({ ...editor, draft: { ...editor.draft, [field.name]: next } });
    const inputId = `edit-${field.name}`;
    return <div className="field" key={field.name}><label htmlFor={inputId}>{field.label}{field.optional ? ' · optional' : ''}</label>
      {field.options ? <select id={inputId} required={!field.optional} value={String(editor.draft[field.name] ?? '')} onChange={e => update(e.target.value)}><option value="">{field.optional ? 'None' : 'Choose…'}</option>{field.options.map(([id, name]) => <option key={id} value={id}>{name}</option>)}</select>
        : field.type === 'checkbox' ? <input id={inputId} type="checkbox" checked={Boolean(editor.draft[field.name])} onChange={e => update(e.target.checked)} />
        : field.type === 'textarea' ? <textarea id={inputId} required={!field.optional} maxLength={500} value={String(editor.draft[field.name] ?? '')} onChange={e => update(e.target.value)} />
        : <input id={inputId} type={field.type ?? 'text'} required={!field.optional} min={field.min} max={field.max} step={field.step} minLength={field.type === 'password' ? 12 : undefined} maxLength={field.type === 'password' ? 128 : field.name === 'email' ? 254 : 120} autoComplete={field.type === 'password' ? 'new-password' : undefined} value={String(editor.draft[field.name] ?? '')} onChange={e => update(e.target.value)} />}
    </div>;
  }
  const locked = busy || !!pending;
  if (!session) return <main className="learning-login"><section className="login-intro"><span className="brandMark">N</span><p className="eyebrow">PEOPLE · STRUCTURE · TIME</p><h1>A clear view of your workplace.</h1><p>Nethris brings your directory, organization and time-off reviews into one local workspace.</p><div className="intro-icons"><UsersRound /><Building2 /><CalendarCheck /></div><p className="muted">Learning workshop · fictional data only</p></section><section className="login-panel"><h2>{initialized === false ? 'Create your workshop' : 'Welcome back'}</h2><p>{initialized === false ? 'Choose your first Admin account. No default password is created.' : 'Sign in with an account from this workshop.'}</p>{error && <p role="alert" className="error">{error}</p>}{notice && <p role="status">{notice}</p>}{initialized === undefined ? <button onClick={() => void setupStatus()}>Retry connection</button> : <form onSubmit={signIn}><fieldset disabled={busy}>{!initialized && <><label>Company name<input name="companyName" required maxLength={120} /></label><div className="form-grid"><label>First name<input name="firstName" required maxLength={80} /></label><label>Last name<input name="lastName" required maxLength={80} /></label></div></>}<label>Email<input name="email" type="email" required autoComplete="username" maxLength={254} /></label><label>Password<input name="password" type="password" required minLength={12} maxLength={128} autoComplete={initialized ? 'current-password' : 'new-password'} /></label>{!initialized && <label className="checkline"><input name="seedDemo" type="checkbox" defaultChecked /> Add five fictional colleagues. Their accounts use this workshop password.</label>}<button className="primary" type="submit">{busy ? 'Please wait…' : initialized ? 'Sign in' : 'Create workshop'}</button></fieldset></form>}<p className="muted">Sessions last up to eight hours. Reloading this page requires signing in again.</p></section></main>;
  const tabs: Tab[] = ['employees', 'org-chart', 'departments', 'job-roles', 'pto', ...(canHr ? ['audit-logs' as Tab] : []), ...(admin ? ['accounts' as Tab] : [])];
  return <div className="workshop-shell"><aside className="workshop-sidebar"><div className="brand"><span className="brandMark">N</span><div><strong>Nethris</strong><small>{company}</small></div></div><p className="eyebrow">YOUR WORKPLACE</p><nav aria-label="Workspace">{tabs.map(item => <button key={item} disabled={locked || !!editor} aria-current={tab === item ? 'page' : undefined} onClick={() => { setTab(item); setOffset(0); setSearch(''); setFilter(''); }}>{item === 'employees' ? <UsersRound size={18} /> : item === 'pto' ? <CalendarCheck size={18} /> : item === 'accounts' ? <ShieldCheck size={18} /> : <ClipboardList size={18} />}{labels[item]}</button>)}</nav><div className="session-box"><strong>{session.user.name}</strong><span>{session.user.role}</span><button disabled={locked} onClick={() => void signOut()}><LogOut size={16} /> Sign out</button></div></aside><main className="workshop-main"><header><div><p className="eyebrow">NETHRIS WORKSPACE</p><h1>{labels[tab]}</h1><p className="muted">{tab === 'pto' ? 'Request time, follow reviews, and keep balances accurate.' : tab === 'employees' ? 'The people and relationships that make your organization.' : 'Keep shared information clear and current.'}</p></div><button disabled={locked} onClick={() => void refresh().then(updated => { if (updated) setNotice('Refreshed.'); }).catch(e => setError(message(e)))}>Refresh</button></header>
    {notice && <p className="notice" role="status">{notice}</p>}{error && <p role="alert" className="error">{error}</p>}
    {pending && !busy && <section className="recovery"><h2>Recover this save</h2><p>The result is uncertain. Retry sends the same request and operation key, so a committed change is returned without repeating it. Keep this page open.</p><button className="primary" onClick={() => void perform(pending)}>Retry exact operation</button><button onClick={() => { setPending(undefined); setEditor(undefined); setNotice('Retry stopped. Refresh and inspect saved records before making a similar change.'); void refresh().catch(e => setError(message(e))); }}>Stop retrying and inspect records</button></section>}
    {editor && <section className="editor" aria-labelledby="editor-heading"><div className="editor-title"><h2 id="editor-heading">{editor.action === 'save' ? editor.row ? 'Edit' : 'Create' : editor.action === 'decision' ? 'Review' : editor.action === 'cancel' ? 'Cancel' : 'Delete'} {labels[editor.kind].toLowerCase()}{editor.row ? ` · ${title(editor.row)}` : ''}</h2><button disabled={locked} onClick={() => { setEditor(undefined); setStale(false); setError(''); }}>Close editor</button></div>
      {stale && <div className="recovery"><h3>Review the latest saved record</h3><p>Your draft is preserved. Compare it with the current values below before trying again.</p>{latest ? <><dl className="comparison">{fields(editor.kind, editor.row).filter(f => f.type !== 'password').map(f => <div key={f.name}><dt>{f.label}</dt><dd>Saved: {String(latest[f.name] ?? 'None')} · Draft: {String(editor.draft[f.name] ?? 'None')}</dd></div>)}</dl><button onClick={() => { setEditor({ ...editor, row: latest }); setStale(false); setError(''); }}>Use latest version, keep draft</button></> : <p>The record could not be loaded. Close the editor and refresh to check whether it was removed or access changed.</p>}</div>}
      <form onSubmit={submit}><fieldset disabled={locked || stale}><div className="form-grid">{editor.action === 'save' ? fields(editor.kind, editor.row).map(fieldInput) : editor.action === 'decision' ? [fieldInput({ name: 'decision', label: 'Decision', options: [['Approved', 'Approve'], ['Rejected', 'Reject']] }), fieldInput({ name: 'comment', label: 'Review comment', type: 'textarea', optional: true })] : <p>{editor.action === 'delete' ? 'Delete this unreferenced record? Records with linked history are protected.' : 'Cancel this pending request? Existing decisions remain in its history.'}</p>}</div>{editor.kind === 'pto' && editor.action === 'save' && <p className="muted">Quarter-hour increments; up to eight hours per calendar date and 31 dates per request. Pending requests reserve your balance. Weekends count as calendar dates.</p>}<button type="submit" className={editor.action === 'delete' ? 'danger' : 'primary'}>{busy ? 'Saving…' : editor.action === 'save' ? 'Save changes' : editor.action === 'decision' ? 'Submit decision' : editor.action === 'cancel' ? 'Confirm cancellation' : 'Confirm deletion'}</button></fieldset></form></section>}
    {tab !== 'org-chart' && <div className="toolbar"><form onSubmit={e => { e.preventDefault(); setFilter(search); setOffset(0); }}><label className="sr-only" htmlFor="search">Search records</label><input id="search" placeholder="Search records…" value={search} disabled={locked || !!editor} onChange={e => setSearch(e.target.value)} /><button disabled={locked || !!editor}>Search</button></form>{(tab === 'pto' || (canHr && ['employees', 'departments', 'job-roles'].includes(tab)) || (admin && tab === 'accounts')) && <button className="primary" disabled={locked || !!editor} onClick={() => open(tab as Entity)}>+ {tab === 'pto' ? 'Request time off' : 'Add record'}</button>}</div>}
    {tab === 'org-chart' ? <section className="org-panel"><h2>Reporting relationships</h2>{org.length ? <ul className="org-tree">{org.map(node => <OrgBranch key={node.id} node={node} />)}</ul> : <p>No reporting relationships to display.</p>}</section> : <><p className="result-count">{page.total} records · showing {page.items.length ? offset + 1 : 0}–{offset + page.items.length}</p><section className="record-grid" aria-label={labels[tab]}>{page.items.map(row => <article className="record" key={row.id}><div className="record-heading"><div className="avatar">{(title(row) || value(row, 'action')).slice(0, 2).toUpperCase()}</div><div><h2>{title(row) || value(row, 'action')}</h2><p className="muted">{tab === 'employees' ? `${value(row, 'jobRole')} · ${value(row, 'department')}` : tab === 'pto' ? value(row, 'status') : tab === 'accounts' ? value(row, 'role') : tab === 'audit-logs' ? value(row, 'entityType') : value(row, 'level') || value(row, 'costCenter')}</p></div></div>
      <dl>{(tab === 'employees' ? [['Email', 'email'], ['Location', 'location'], ['Manager', 'managerName'], ['Status', 'status'], ['PTO hours', 'ptoBalanceHours']] : tab === 'pto' ? [['From', 'startDate'], ['Through', 'endDate'], ['Hours', 'hours'], ['Reason', 'reason']] : tab === 'departments' ? [['People', 'headcount'], ['Lead', 'leadName'], ['Active', 'isActive']] : tab === 'job-roles' ? [['Minimum salary', 'salaryBandMin'], ['Maximum salary', 'salaryBandMax'], ['Active', 'isActive']] : tab === 'accounts' ? [['Email', 'email'], ['Enabled', 'isActive']] : [['Actor', 'actorEmail'], ['When', 'createdAt'], ['Record', 'entityId']]).map(([label, key]) => <div key={key}><dt>{label}</dt><dd>{row[key] == null ? (key.includes('salary') || key === 'ptoBalanceHours' ? 'Restricted' : '—') : value(row, key)}</dd></div>)}</dl>
      {tab === 'pto' && Array.isArray(row.approvals) && row.approvals.length > 0 && <ul className="approval-history">{(row.approvals as { id: string; stage: string; approverName: string; decision: string; comment?: string }[]).map(a => <li key={a.id}>{a.stage}: {a.decision} by {a.approverName}{a.comment && ` — ${a.comment}`}</li>)}</ul>}
      <div className="record-actions">{tab !== 'audit-logs' && tab !== 'pto' && canHr && (tab !== 'accounts' || admin) && <><button disabled={locked || !!editor} onClick={() => open(tab as Entity, row)}>Edit</button>{tab !== 'accounts' && <button disabled={locked || !!editor} onClick={() => open(tab as Entity, row, 'delete')}>Delete</button>}</>}
      {tab === 'pto' && <>{row.employeeId === session.user.employeeId && ['PendingManager', 'PendingHr'].includes(value(row, 'status')) && <>{Array.isArray(row.approvals) && row.approvals.length === 0 && <><button disabled={locked || !!editor} onClick={() => open('pto', row)}>Edit</button><button disabled={locked || !!editor} onClick={() => open('pto', row, 'delete')}>Delete</button></>}<button disabled={locked || !!editor} onClick={() => open('pto', row, 'cancel')}>Cancel request</button></>}{row.employeeId !== session.user.employeeId && ((row.status === 'PendingManager' && row.assignedManagerId === session.user.employeeId && rank(session.user.role) >= 1) || (row.status === 'PendingHr' && canHr && !(row.approvals as { approverEmployeeId: string }[]).some(a => a.approverEmployeeId === session.user.employeeId))) && <button disabled={locked || !!editor} onClick={() => open('pto', row, 'decision')}>Review request</button>}</>}
      </div></article>)}</section>{!page.items.length && <div className="empty-state"><h2>No records here yet</h2><p>Try a different search or add a record when your role allows it.</p></div>}<div className="pagination"><button disabled={locked || !!editor || offset === 0} onClick={() => setOffset(Math.max(0, offset - 12))}>Previous page</button><span>Page {Math.floor(offset / 12) + 1}</span><button disabled={locked || !!editor || offset + 12 >= page.total} onClick={() => setOffset(offset + 12)}>Next page</button></div></>}
    <footer>Local HRIS learning workshop · Changes persist in your workshop database.</footer></main></div>;
}
function OrgBranch({ node }: { node: Org }) { return <li><div className="org-person"><strong>{node.name}</strong><span>{node.title} · {node.department}</span></div>{node.reports.length > 0 && <ul>{node.reports.map(child => <OrgBranch key={child.id} node={child} />)}</ul>}</li>; }
