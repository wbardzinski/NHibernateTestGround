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
            Console.ReadLine();
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
