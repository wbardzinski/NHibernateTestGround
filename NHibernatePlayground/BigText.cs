using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernatePlayground
{
    public class BigTextMap : ClassMap<BigText>
    {
        public BigTextMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Message).CustomType("StringClob").Not.Nullable();
            Map(x => x.Number);
        }
    }

    public class BigText
    {
        public BigText()
        {
        
        }
        public virtual int Id { get; set; }
        public virtual string Message { get; set; }
        public virtual int Number { get; set; }
    }
}
