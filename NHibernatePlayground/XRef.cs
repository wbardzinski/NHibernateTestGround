using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernatePlayground
{
    public class XRefMap : ClassMap<XRef>
    {
        public XRefMap()
        {
            Table("XRef");
            Not.LazyLoad();

            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.Type);
        }
    }
    public class XRef
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }
}
