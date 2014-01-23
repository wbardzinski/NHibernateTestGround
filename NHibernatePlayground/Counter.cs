using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernatePlayground
{
    public class CounterMap : ClassMap<Counter>
    {
        public CounterMap()
        {
            OptimisticLock.Dirty();
            DynamicUpdate();

            Id(x => x.Id).GeneratedBy.Identity().Unique();
            Map(x => x.NumberId).Not.Nullable();
            Map(x => x.Current).Not.Nullable().OptimisticLock();
            Map(x => x.Parameter1).Nullable();
        }
    }
    public class Counter
    {
        public virtual int Id { get; set; }
        public virtual int NumberId { get; set; }
        public virtual int Current { get; set; }
        public virtual string Parameter1 { get; set; }
    }
}
