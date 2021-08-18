using System.Linq;
using NHibernate.Cfg;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH2866
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty("hbm2ddl.keywords", "auto-quote");
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var user = new User();
				var tenant = new Tenant();
				var tenantUser = new TenantUser {User = user, Tenant = tenant};
				tenantUser.TenantUserPerson = new TenantUserPerson {TenantUser = tenantUser};
				user.TenantUsers.Add(tenantUser);

				session.Save(user);

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				// The HQL delete does all the job inside the database without loading the entities, but it does
				// not handle delete order for avoiding violating constraints if any. Use
				// session.Delete("from System.Object");
				// instead if in need of having NHibernate ordering the deletes, but this will cause
				// loading the entities in the session.
				session.CreateQuery("delete from TenantUserPerson").ExecuteUpdate();
				session.CreateQuery("delete from TenantUser").ExecuteUpdate();
				session.CreateQuery("delete from Tenant").ExecuteUpdate();
				session.CreateQuery("delete from User").ExecuteUpdate();

				transaction.Commit();
			}
		}

		[Test]
		public void Delete_By_Orphaning_TenantUser()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var user = session.QueryOver<User>().SingleOrDefault();
				user.TenantUsers.Clear();

				transaction.Commit();
			}

			using (var session = OpenSession()) Assert.That(session.QueryOver<TenantUserPerson>().List(), Is.Empty);
		}

		[Test]
		public void Delete_By_Orphaning_TenantUserPerson()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var user = session.QueryOver<User>().SingleOrDefault();
				var tenantUser = user.TenantUsers.First();
				tenantUser.TenantUserPerson.TenantUser = null;
				tenantUser.TenantUserPerson = null;

				transaction.Commit();
			}

			using (var session = OpenSession()) Assert.That(session.QueryOver<TenantUserPerson>().List(), Is.Empty);
		}
	}
}
