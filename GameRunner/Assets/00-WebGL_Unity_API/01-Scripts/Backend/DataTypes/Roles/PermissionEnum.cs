using System;

namespace Cohort.Ravel.Permissions
{
	public enum SystemRole
	{
		NONE    = 0,
		GUEST   = 1,
		USER    = 2,
		MANAGER = 3,
		SUPER   = 4,
		ADMIN   = 5,
	}
	
	[Flags]
	public enum Permission {
		None = 0,
		CONTROL_PORTAL			= 1<<0,
		MUTE_USER_VIDEO			= 1<<1,
		MUTE_USER_AUDIO			= 1<<2,
		MUTE_ALL_AUDIO			= 1<<3,
		MOVE_CONTENT			= 1<<4,
		CREATE_POLL				= 1<<5,
		CONTROL_PRESENTATION	= 1<<6,
		CLEAR_SPACE				= 1<<7,
		CHANGE_ROLE				= 1<<8,
		RESTRICTED_AREA_ACCESS	= 1<<9,
		CONTROL_VIEWPOINTS		= 1<<10,
		SEATING					= 1<<11,
		QUIZ_MODERATION			= 1<<12,
		CONTROL_GAME			= 1<<13,
		DEV			            = 1<<14,
	}
}