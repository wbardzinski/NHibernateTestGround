using FluentNHibernate.Cfg;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NHibernate.Linq;
using System.Transactions;
using System.Data;
using System.Data.SqlClient;
using NHibernate.Dialect;
using NHibernate.Dialect.Function;
using System.Linq.Expressions;
using NHibernate.Engine;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Id.Enhanced;
using System.Diagnostics;
using NodaTime;
namespace NHibernatePlayground
{
    public class CustomFunctionsMsSql2008Dialect : MsSql2008Dialect
    {
        public CustomFunctionsMsSql2008Dialect()
        {
            RegisterFunction("rowcount", new NoArgSQLFunction("count(*) over",
                   NHibernateUtil.Int32, true));
        }
    }

    public static class SessionHelper
    {
        public static void Delete<TEntity>(this ISession session, object id)
        {
            var queryString = string.Format("delete {0} where id = :id",
                                            typeof(TEntity));
            session.CreateQuery(queryString)
                   .SetParameter("id", id)
                   .ExecuteUpdate();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            TestNodaTime();
            //TestBatching();
            //TestLinq();
            //TestRowCount();
            //TestBigText();
            //TestDecimalMoney();
            //TestVersion();
            //TestDelete();
            //TestCounter();
            //TestConstValueReference();
            //TestAggregatesJoinFetching();
            Console.ReadLine();
        }

        private static void TestNodaTime()
        {
            var factory = BuildSessionFactory();
            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var list = session.Query<Meeting>().Where(m=>m.Time < new LocalTime(8,0,0)).ToList();
                list = session.Query<Meeting>().Where(m => m.Date < new LocalDate(2012, 1, 1)).ToList();
                var mt = new Meeting()
                {
                    Date = new NodaTime.LocalDate(1911, 11, 11),
                    Time = NodaTime.SystemClock.Instance.Now.InZone(NodaTime.DateTimeZoneProviders.Tzdb.GetSystemDefault()).TimeOfDay
                };
                session.Save(mt);
                tx.Commit();
            }
        }

        private static void TestBatching()
        {
            var factory = BuildSessionFactory();
            var sessions = (ISessionFactoryImplementor)factory;
            sessions.Statistics.Clear();

            var persister = sessions.GetEntityPersister(typeof(Product).FullName);

            var generator = (TableGenerator)persister.IdentifierGenerator;
            var optimizer = (OptimizerFactory.HiLoOptimizer)generator.Optimizer;

            var s = Stopwatch.StartNew();
            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                for (int i = 0; i < 100; i++)
                {
                    var p = new Product() { Name = "Test", Description = "Test"/*, Id =1 */};
                    session.Save(p);
                    p = new Product() { Name = "Test1", Description = "Test1"/*, Id = 2 */};
                    session.Save(p);
                    var r = new Release()
                    {
                        Product = p,
                        Name = "Test",

                    };
                    session.Save(r);
                }
                tx.Commit();
            }
            s.Stop();
            var ms = s.ElapsedMilliseconds;

            var count = sessions.Statistics.PrepareStatementCount;
        }

        private static void TestLinq()
        {
            var factory = BuildSessionFactory();
            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var p = session.Query<Product>().Fetch(pr => pr.Releases).Where(pr => pr.Id > 0).First();
                var expr = Expression.Lambda<Func<Release, bool>>(
                    Expression.MakeBinary(ExpressionType.Equal, 
                        Expression.MakeMemberAccess(Expression.Parameter(typeof(Release), "rel"), typeof(Release).GetProperty("Product"))
                    , 
                    Expression.Constant(p)),
                    Expression.Parameter(typeof(Release), "rel"));
                
                //Expr<Release>(r => r.Product == p);
                var rel = session.Query<Release>().FirstOrDefault(expr);
            }
        }

        
        private static void Expr<T>(Expression<Func<T, bool>> expr)
        {
            expr.Compile();
            var be = expr.Body as BinaryExpression;
            var fe = be.Right as MemberExpression;
        }

        private static void TestRowCount()
        {
            var factory = BuildSessionFactory();
            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var x = session.CreateQuery("select b from BigText b").List();
                var l1 = session.QueryOver<BigText>()
    .List();

                var li2 = session.QueryOver<BigText>()
                    .Select(t => t.Id)
                    .List<int>();
                var li = session.QueryOver<BigText>()
                    .SelectList(l => l
                        .Select(t => t.Id)
                        .Select(t => t.Message))
                    .List<object[]>();
            }
        }

        private static void TestBigText()
        {
            var factory = BuildSessionFactory();
            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var bt = new BigText();
                bt.Message = new String('a', 10001);
                session.Save(bt);
                tx.Commit();
            }
        }

        private static void TestDecimalMoney()
        {
            var factory = BuildSessionFactory();
            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var p = session.Query<Release>().First();
                p.Cost = 31.4111m;
                p.AverageCost = 11.4312m;
                p.TotalCost = 12.3423m;
                tx.Commit();
            }
        }

        private static void TestVersion()
        {
            var factory = BuildSessionFactory();
            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var p = session.Load<Product>(2002);
                var r = new Release()
                {
                    Product = p,
                    Name = "R1"
                };
                session.Save(r);
                tx.Commit();
            }
        }

        private static void TestDelete()
        {
            var factory = BuildSessionFactory();
            object id = null;
            using (var session = factory.OpenSession())
                using (var tx = session.BeginTransaction())
                {
                    var product = new Product()
                    {
                        Description = "BO",
                        Name = "PR",
                    };
                    product.Releases.Add(new Release() { Name = "Rel1", Product = product });
                    product.Releases.Add(new Release() { Name = "Rel2", Product = product });
                    id = session.Save(product);
                    tx.Commit();
                }
            using (var session = factory.OpenSession())
                using (var tx = session.BeginTransaction())
                {
                    var product = session.Get<Product>(id);
                    session.CreateQuery("delete from Release where Product=:product")
                        .SetParameter("product", product)
                        .ExecuteUpdate();
                    session.Delete(product);
                    tx.Commit();
                }
        }

        private static void TestCounter()
        {
            var factory = BuildSessionFactory();
            int numberId = 1;
            

            Action<ISession> a = session =>
            {
                using (var tx = session.BeginTransaction())
                {
                    var counter = session.Query<Counter>()
                        .Where(c => c.NumberId == numberId)
                        .SingleOrDefault();
                    if (counter == null)
                    {
                        counter = new Counter()
                        {
                            NumberId = numberId,
                            Current = 1
                        };
                        session.Save(counter);
                    }
                    else
                    {
                        counter.Current++;
                        session.Update(counter);
                    }
                    /*
                    session.CreateQuery("update Counter c set c.Current = 1 + c.Current where c.NumberId=:numberId")
                        .SetInt32("numberId", numberId)
                        .ExecuteUpdate();
                    var result = session.CreateQuery("select Current from Counter where NumberId=:numberId")
                        .SetInt32("numberId", numberId)
                        .UniqueResult<int>();*/
                    tx.Commit();
                }
            };
            var sess = factory.OpenSession();
            var success = false;
            while (!success)
            {
                //using (var tx = new TransactionScope())
                {
                    try
                    {
                        
                        using (var transaction = sess.BeginTransaction())
                        {
                            string par1 = null;
                            // build an ADO command
                            var command = new SqlCommand()
                            {
                                Parameters = 
                                {
                                    new SqlParameter("@numberId", 1),
                                    new SqlParameter("@par1", SqlDbType.VarChar, 50) {Value = (object)par1 ?? DBNull.Value},
                                    new SqlParameter("@counter", SqlDbType.Int) 
                                    {
                                        Direction = ParameterDirection.Output
                                    }
                                },
                                CommandType = CommandType.StoredProcedure,
                                CommandText = "dbo.NextCounter"
                            };

                            // give it the connection we're using from NH
                            (command as IDbCommand).Connection = sess.Connection;
                            // tell the NH transaction to use the ADO command
                            transaction.Enlist(command);

                            // execute the command
                            command.ExecuteNonQuery();

                            var val = command.Parameters["@counter"].Value.As<int>();
                            // grab the return value casting it appropriately
                            //return (int)((SqlParameter)command.Parameters["@OutputParam"]).Value);
                        }
                        /*
                        var ctr = sess.CreateSQLQuery("EXEC [dbo].[NextCounter] @numberId=:numberId, @counter=:counter")
                            .SetInt32("numberId", 1)
                            .SetInt32("counter", 0)
                            .UniqueResult<int>();
                        var ctr = sess.GetNamedQuery("NextCounter")
                            .SetInt32("numberId", 1)
                            .SetInt32("counter", 0)
                            .UniqueResult<int>();
                        */

                        a(sess);
                        success = true;
                    }
                    catch (Exception e)
                    {
                        sess.Clear();
                    }
                    //tx.Complete();
                }
            }
        }

        private static void TestAggregatesJoinFetching()
        {
            var factory = BuildSessionFactory();
            int id;
            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var product = new Product() { Name = "EvoBO", Description = "New product" };
                product.Releases.Add(new Release() { Name = "Release 1" });
                id = (int)session.Save(product);
var backlogItem = new BacklogItem() { Name = "Faktury", ProductId = id };
                session.Save(backlogItem);

                tx.Commit();
            }

            using (var session = factory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                var productFuture = session.QueryOver<Product>()
                    .Where(p => p.Id == id)
                    .FutureValue();
                var backlogItems = session.QueryOver<BacklogItem>()
                    .Where(bi => bi.ProductId == id)
                    .Future();
                var product = productFuture.Value;
                foreach (var bi in backlogItems)
                {
                    Console.WriteLine(bi.Name);
                }
                tx.Commit();
            }
        }

        private static void TestConstValueReference()
        {
            var factory = BuildSessionFactory();
            object id;
            using (var session = factory.OpenSession())
            {
                var doc = new XDocClass();
                doc.Document = XDocument.Parse("<MyNode>my Text</MyNode>");
                id = session.Save(doc);
                session.Flush();
            }
            using (var session = factory.OpenSession())
            {
                var doc = session.Get<XDocClass>(id);
                Console.WriteLine(doc.Document.ToString());
                session.Delete(doc);
                session.Flush();
            }
        }

        private static ISessionFactory BuildSessionFactory()
        {
            //keywords import and auto-quote is configured in hibernate.cfg.xml
            try
            {
                var nhConfig = Fluently.Configure(new Configuration().Configure())
                    .Mappings(mappings =>
                    {
                        mappings.FluentMappings.AddFromAssemblyOf<Program>();
                        mappings.HbmMappings.AddFromAssemblyOf<Program>();
                    })
                    .BuildConfiguration();
                new SchemaExport(nhConfig).Create(true, false);
                return nhConfig.BuildSessionFactory();
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(Program)).Error(e.Message);
                return null;
            }
        }
    }
}
