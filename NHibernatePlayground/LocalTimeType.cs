using NHibernate;
using NHibernate.Dialect;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernatePlayground
{
    [Serializable]
    public class LocalTimeType : PrimitiveType, IIdentifierType
    {
        private readonly LocalTimePattern _timePattern = LocalTimePattern.CreateWithInvariantCulture("h:mm:ss tt");
        private static readonly DateTime BaseDateValue = new DateTime(1753, 01, 01);

        public LocalTimeType()
            : base(SqlTypeFactory.Time)
        {
        }

        public override string Name
        {
            get { return "LocalTime"; }
        }

        public override object Get(IDataReader rs, int index)
        {
            try
            {
                if (rs[index] is TimeSpan) //For those dialects where DbType.Time means TimeSpan.
                {
                    var time = (TimeSpan)rs[index];
                    return LocalTime.Midnight + Period.FromTicks(time.Ticks);
                }

                var dbValue = Convert.ToDateTime(rs[index]);
                return LocalDateTime.FromDateTime(dbValue).TimeOfDay;
            }
            catch (Exception ex)
            {
                throw new FormatException(string.Format("Input string '{0}' was not in the correct format.", rs[index]), ex);
            }
        }

        public override object Get(IDataReader rs, string name)
        {
            return Get(rs, rs.GetOrdinal(name));
        }

        public override Type ReturnedClass
        {
            get { return typeof(LocalTime); }
        }

        public override void Set(IDbCommand st, object value, int index)
        {
            var parameter = ((SqlParameter)st.Parameters[index]);
            var localTime = (LocalTime)value;
            parameter.DbType = DbType.Time; // HACK work around bad behavior, M$ says not ideal, but as intended, NH says this is a bug in MS may work around eventually
            parameter.Value = BaseDateValue + new TimeSpan(localTime.TickOfDay);
        }

        public override bool IsEqual(object x, object y)
        {
            return Equals(x, y);
        }

        public override int GetHashCode(object x, EntityMode entityMode)
        {
            return x.GetHashCode();
        }

        public override string ToString(object val)
        {
            return _timePattern.Format((LocalTime)val);
        }

        public object StringToObject(string xml)
        {
            return string.IsNullOrEmpty(xml) ? null : FromStringValue(xml);
        }

        public override object FromStringValue(string xml)
        {
            return _timePattern.Parse(xml).Value;
        }

        public override Type PrimitiveClass
        {
            get { return typeof(LocalTime); }
        }

        public override object DefaultValue
        {
            get { return new LocalTime(); }
        }

        public override string ObjectToSQLString(object value, Dialect dialect)
        {
            return "'" + _timePattern.Format((LocalTime)value) + "'";
        }
    }
}
