# 06. Reference lifecycles and safe deletion

Departments and job roles are reference records used by employees. Making one inactive is different from deleting it. Read `SaveEmployee`, `SaveDepartment`, `SaveJobRole`, and `Delete` in [HrisPeople.cs](../../../server/Nethris.Api/Domain/HrisPeople.cs), together with relationship delete behavior in [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs). The application preserves existing references while restricting new assignments to inactive records.

## Existing assignment versus new assignment

The employee save rule rejects an inactive department when the employee is being assigned to a different department identity. It similarly rejects a newly selected inactive job role. If the employee already has that same inactive reference, the comparison permits retaining it while editing another field. This is a practical compatibility policy: deactivating a reference does not make every existing employee impossible to edit.

Use an employee already assigned to department D. Deactivate D, then edit the employee's location without changing DepartmentId. Contrast that with assigning a different employee to D for the first time. The visible department name is the same, but the relationship history differs. A test matrix must include both cases to establish the intended rule.

## Department leadership adds another relationship

A department lead must be a non-terminated employee of that department. An employee who leads a department cannot be moved to another department until leadership is cleared. These rules prevent a department from retaining a lead who no longer belongs to it. They also create a deliberate multi-step user workflow: clear or change leadership, then move the employee.

Do not bypass that workflow by changing only the employee's department in the database. The learning objective is to understand the supported relationship transition. A future combined move-and-reassign-lead command could be useful, but it would need its own atomic contract, authorization, version expectations, and tests. It is a new feature rather than a harmless shortcut.

## Deletion checks preserve history

Employee deletion checks several references: account, direct reports, department leadership, PTO request ownership or assigned management, and approval history. A record can be unreferenced by the current organization chart yet still be referenced by historical approvals. Deletion must account for both current structure and recorded history. The application suggests resolving relationships or archiving appropriate reference records rather than removing history indiscriminately.

Department and job-role deletion check employee references. The database also uses restrictive delete behavior for these relationships. Domain checks provide understandable refusals, while relational constraints protect storage integrity. A direct database constraint failure is not a substitute for a useful domain message, but the message alone should not be the only protection.

## Exercise NH-06A: build a reference-state matrix

Create rows for active existing reference, inactive existing reference, active new reference, inactive new reference, and nonexistent reference. Predict employee-edit outcomes while keeping other fields valid. Include a control where only location changes. Explain why a blanket reject inactive references rule would break that valid maintenance path.

Then inspect the selector UI and describe how it can represent an existing inactive selection without offering it as a new choice. If the current interface does not fully express that distinction, record it as an improvement proposal. Do not assume the backend rule alone guarantees an understandable form experience.

## Exercise NH-06B: plan an employee transfer

An employee leads department A and needs to move to department B. Write the supported sequence, including versions loaded, leadership change, employee edit, and stale-conflict recovery. Decide what happens if another HR user edits the department between steps. A multi-step workflow can partially complete by design; the UI should make the current accepted state clear.

Now propose an atomic transfer feature as an independent extension. Specify both expected versions and the outcome if either is stale. Identify the transaction boundary that would cover leadership and employee movement together. Compare its complexity with the existing explicit sequence. Your proposal should explain the product value, not merely prefer fewer clicks.

## Exercise NH-06C: review a deletion request

Given an employee with no direct reports but one historical PTO approval, predict deletion. Explain which reference matters and why a query looking only at current staff would be insufficient. Create an isolated fixture with one reference at a time so each rejection check is meaningfully exercised.

For a proposed archival feature, define what remains visible in historical projections. Renaming or deactivating a person can affect displayed labels while stable identifiers preserve relationships. Decide whether an audit or approval view should show current names or captured historical names. The current source may use related current names; a historical snapshot policy would require new stored data and migration considerations.

## Review standard

The central skill is lifecycle modeling. Active, inactive, referenced, historical, and deleted are different states. A good solution preserves valid existing relationships, blocks invalid new ones, and explains the user's path through constrained transitions. Your evidence should include both a successful maintenance edit with an inactive existing reference and a rejected destructive action that would break history.
