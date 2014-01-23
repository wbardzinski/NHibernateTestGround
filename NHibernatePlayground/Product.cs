using System;
using System.Collections.Generic;
using Iesi.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using NHibernate.Id.Enhanced;

namespace NHibernatePlayground
{
    public class ProductMap : ClassMap<Product>
    {
        public ProductMap()
        {
            var n = this.GetType().BaseType.GenericTypeArguments[0].Name;
            Id(x => x.Id).Unique().GeneratedBy
                .Custom<TableGenerator>(p => p
                    .AddParam(TableGenerator.OptimizerParam, "hilo")
                    .AddParam(TableGenerator.SegmentValueParam, n)
                    .AddParam(TableGenerator.InitialParam, "1")
                    .AddParam(TableGenerator.IncrementParam, "10")
                    )
                .Not.Nullable();
            //.HiLo("nhibernate_ids", "next_id", "100", "TableName = 'Product'").Not.Nullable();
            Version(x => x.Version);
            OptimisticLock.Version();
            DynamicUpdate();
            Map(x => x.Name).Not.Nullable();
            Map(x => x.Description);
            HasMany(x => x.Releases).LazyLoad().AsSet().KeyColumn("ProductId").Cascade.AllDeleteOrphan().Inverse();
        }
    }

    public class Product
    {
        public Product()
        {
            Releases = new HashedSet<Release>();
        }
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual Iesi.Collections.Generic.ISet<Release> Releases { get; set; }
        public virtual int Version { get; set; }
    }
}
