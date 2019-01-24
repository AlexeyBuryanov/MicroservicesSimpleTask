*******************************************************************************
                      * Microservices simple task *
*******************************************************************************

UsersStorageService - is used for storing users. 
PermissionsService - is used for storing the permissions list and relations 
between permissions and users. 
ManagementService - is used for managing (add/edit/delete) users and permissions 
and for assigning permissions to users. Should have very simple graphic user 
interface.

///////////////////////////////////////////////////////////////////////////////
/ UsersStorageService 
///////////////////////////////////////////////////////////////////////////////

Business logic ----------------------------------------------------------------
-------------------------------------------------------------------------------

User 
-- Id 
-- FirstName 
-- LastName 
-- Email 
-- Password (should be encrypted)

UserRepository 
-- GetAllUsers() 
-- AddOrReplaceUser(userId, newOrUpdatedUser)
-- DeleteUser()

Public API --------------------------------------------------------------------
-------------------------------------------------------------------------------

GET    http://localhost:5000/users/list - returns all users from the DB.
POST   http://localhost:5000/users/update/{userId} + request body {newOrUpdatedUser} - updates the user 
       if exists or add new user otherwise. 
DELETE http://localhost:5000/users/delete/{userId} - deletes the user by id.

///////////////////////////////////////////////////////////////////////////////
/ PermissionsService 
///////////////////////////////////////////////////////////////////////////////

Business logic ----------------------------------------------------------------
-------------------------------------------------------------------------------

Permission 
-- Id 
-- Name

UserPermissions 
-- UserId 
-- PermissionIds - the list of user's permission IDs.

PermissionUsers 
-- PermissionId 
-- UserIds - the list of the user IDs which have the permission.

PermissionRepository 
-- GetAllPermissions() 
-- GetUserPermissions(userId) 
-- AddOrReplacePermission(permissionId, newOrUpdatedPermission) 
-- DeletePermission(permissionId) 
-- AssignPermission(permissionId, userId) 
-- UnassignPermission(permissionId, userId)

Public API --------------------------------------------------------------------
-------------------------------------------------------------------------------

GET    http://localhost:5001/permissions/list - returns all permissions from the DB. 
GET    http://localhost:5001/permissions/list/{userId} - returns the permissions for the user. 
POST   http://localhost:5001/permissions/update/{permissionId} + request body {newOrUpdatedPermission} - updates the permission 
       if exists or add new user otherwise. 
POST   http://localhost:5001/permissions/assign/{permissionId}/{userId} - assigns the permission to the user. 
POST   http://localhost:5001/permissions/unassign/{permissionId}/{userId} - unassigns the permission out of the user. 
DELETE http://localhost:5001/permissions/delete/{permissionId} - deletes the permission by id.

///////////////////////////////////////////////////////////////////////////////
/ ManagementService 
///////////////////////////////////////////////////////////////////////////////

Users table:
- possibility to add a new user ("inline" or "popup" add mode) 
- possibility to edit the user ("inline" or "popup" edit mode, "Permissions" column is not editable) 
- possibility to delete the user

Permissions table:
- possibility to add a new permission ("inline" or "popup" add mode) 
- possibility to edit the permission ("inline" or "popup" edit mode) 
- possibility to delete the permission

Assign/Unassign Permissions table:
- here must be possible to add/delete 
- the permissions the result must be shown in the "Users" table ("Permissions" column)

TODO list:
- Indexes
- Unit tests
- Replace the HTTP interaction protocol between ManagementService and PermissionsService or ManagementService and Users StorageService by MQTT. 