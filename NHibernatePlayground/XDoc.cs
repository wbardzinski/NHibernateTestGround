using FluentNHibernate;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NHibernatePlayground
{
    public class XDocClassMap : ClassMap<XDocClass>
    {
        public XDocClassMap()
        {
            Table("XDoc");
            Not.LazyLoad();

            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.Document);
            Map(x => x.AutoDocument);
            Map(Reveal.Member<XDocClass>("Type")).Formula("'AST'");

            HasMany(x => x.Refs).KeyColumn("Type").PropertyRef("Type").Fetch.Join().Cascade.None().ReadOnly().Inverse();//.Inverse().Not.ForeignKeyCascadeOnDelete();
        }
    }
    public class XDocClass
    {
        public int Id { get; set; }
        public XDocument Document { get; set; }
        public XDocument AutoDocument { get; set; }
        public IList<XRef> Refs { get; set; }
        private string Type { get; set; }
    }
}
