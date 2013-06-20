using System;
using System.Collections.Generic;
using Iesi.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace NHibernatePlayground
{
    public class ProductMap : ClassMap<Product>
    {
        public ProductMap()
        {
            Not.LazyLoad();
            Id(x => x.Id).Unique().GeneratedBy.Identity().Not.Nullable();
            Map(x => x.Name).Not.Nullable();
            Map(x => x.Description);
            HasMany(x => x.Releases).AsSet().KeyColumn("ProductId").Cascade.AllDeleteOrphan();
        }
    }

    public class Product
    {
        public Product()
        {
            Releases = new HashedSet<Release>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Iesi.Collections.Generic.ISet<Release> Releases { get; set; }
    }
}
