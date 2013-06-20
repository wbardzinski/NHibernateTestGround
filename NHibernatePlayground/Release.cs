using FluentNHibernate.Mapping;
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
            Not.LazyLoad();
            Id(x => x.Id).Unique().GeneratedBy.Identity().Not.Nullable();
            Map(x => x.Name).Not.Nullable();
        }
    }

    public class Release
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
