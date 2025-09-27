namespace Trippio.Core.SeedWorks.Constants
{
    public static class UserClaims
    {
        public const string Id = "id";
        public const string FirstName = "first_name";
        public const string Roles = "roles";
        public const string Permissions = "permissions";
    }

    public static class Roles
    {
        public const string Admin = "admin";
        public const string User = "user";
    }

    public static class Permissions
    {
        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
        }

        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Create = "Permissions.Roles.Create";
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";
        }
    }
}