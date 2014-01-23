using FluentNHibernate.Mapping;
using NHibernate.Id.Enhanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernatePlayground
{
    public class ReleaseMap : ClassMap<Release>
    {
        public ReleaseMap()
        {
            var n = this.GetType().BaseType.GenericTypeArguments[0].Name;
            Id(x => x.Id).Unique().GeneratedBy
                //.HiLo("nhibernate_ids", "next_id", "100", "TableName = 'Release'")
                .Custom<TableGenerator>(p => p.AddParam(TableGenerator.OptimizerParam, "hilo")
                    .AddParam(TableGenerator.SegmentValueParam, n)
                    .AddParam(TableGenerator.InitialParam, "1")
                    .AddParam(TableGenerator.IncrementParam, "10")
                    )
                .Not.Nullable();
            Version(x => x.Version);
            Map(x => x.Name).Not.Nullable();
            References(x => x.Product).Column("ProductId").Nullable();
            Map(x => x.Cost).Precision(7).Scale(4);
            Map(x => x.AverageCost).Precision(7).Scale(4);
            Map(x => x.TotalCost);
        }
    }

    public class Release
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual Product Product { get; set; }
        public virtual decimal Cost { get; set; }
        public virtual decimal AverageCost { get; set; }
        public virtual decimal TotalCost { get; set; }
        public virtual int Version { get; set; }
    }
}
