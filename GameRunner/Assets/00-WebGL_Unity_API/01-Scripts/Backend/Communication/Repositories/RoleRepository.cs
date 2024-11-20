using System;
using UnityEngine;

namespace Cohort.Permissions
{
	public class RoleRepository
	{
		public void GetLocalPermissions(Action<Permission> onComplete, Action<string> onFailure) {
			DataServices.Users.Local.role.ConvertData();

			DataServices.Roles.SetRoleAndPermissions(DataServices.Users.Local.role.permissions);
			onComplete?.Invoke(DataServices.Users.Local.role.permissions);
		}
	}

	[Serializable]
	public class Role
	{
		public string label;

		public string[] SystemRoleNames {
			get { return systemRoles; }
		}

		[SerializeField] private string[] permissionNames;
		[SerializeField] private string[] systemRoles;

		public SystemRole[] system;
		public Permission permissions;

		/// <summary>
		/// Converts string data retrieved from the backend into actual enum values.
		/// </summary>
		public void ConvertData() {
			permissions = Permission.None;
			for (int i = 0; i < permissionNames.Length; i++) {
				if (Permission.TryParse(permissionNames[i], out Permission p)) {
					permissions |= p;
				}
				else {
					Debug.LogError($"Permission ({permissionNames[i]}) not found!");
				}
			}

			system = new SystemRole[systemRoles.Length];
			for (int i = 0; i < systemRoles.Length; i++) {
				if (SystemRole.TryParse(systemRoles[i], out SystemRole s)) {
					system[i] = s;
				}
				else {
					Debug.LogError($"System role ({systemRoles[i]}) not found!");
					system[i] = SystemRole.NONE;
				}
			}

			permissionNames = Array.Empty<string>();
			systemRoles = Array.Empty<string>();
		}
	}
}