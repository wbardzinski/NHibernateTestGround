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

namespace NHibernatePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            //TestConstValueReference();
            TestAggregatesJoinFetching();
            Console.ReadLine();
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
                    .Mappings(mappings => mappings.FluentMappings.AddFromAssemblyOf<Program>())
                    .BuildConfiguration();

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
