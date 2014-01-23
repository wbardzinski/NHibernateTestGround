using FluentNHibernate.Mapping;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernatePlayground
{
    public class MeetingMap : ClassMap<Meeting>
    {
        public MeetingMap()
        {
            Table("Meeting");
            Id(x => x.Id);
            Map(x => x.Date).CustomType<LocalDateType>();
            Map(x => x.Time).CustomType<LocalTimeType>();
        }
    }

    public class Meeting
    {
        public virtual int Id { get; set; }
        public virtual LocalDate Date { get; set; }
        public virtual LocalTime Time { get; set; }
    }
}
