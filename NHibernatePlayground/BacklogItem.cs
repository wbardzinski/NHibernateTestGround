using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernatePlayground
{
    public class BacklogItemMap : ClassMap<BacklogItem>
    {
        public BacklogItemMap()
        {
            Not.LazyLoad();
            Id(x => x.Id).Unique().GeneratedBy.Identity().Not.Nullable();
            Map(x => x.ProductId).Not.Nullable();
            Map(x => x.Name).Not.Nullable();
        }
    }

    public class BacklogItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
    }
}
