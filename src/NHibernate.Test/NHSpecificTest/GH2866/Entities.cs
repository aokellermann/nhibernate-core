using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH2866
{
	public class User
	{
		public virtual long Id { get; set; }

		public virtual ISet<TenantUser> TenantUsers { get; protected set; } = new HashSet<TenantUser>();
	}


	public class Tenant
	{
		public virtual long Id { get; set; }
	}


	public class TenantUserPerson
	{
		public virtual long Id { get; set; }

		public virtual TenantUser TenantUser { get; set; }
	}


	public class TenantUser
	{
		public virtual long Id { get; set; }

		public virtual User User { get; set; }

		public virtual Tenant Tenant { get; set; }

		public virtual TenantUserPerson TenantUserPerson { get; set; }
	}
}
