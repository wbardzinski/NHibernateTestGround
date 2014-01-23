using NHibernate;
using NHibernate.Dialect;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.UserTypes;
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
    public class LocalDateType : PrimitiveType, IIdentifierType, IParameterizedType
    {
        public const string BaseValueParameterName = "BaseValue";
        public static readonly LocalDate BaseDateValue = new LocalDate(1753, 01, 01);
        private LocalDate customBaseDate = BaseDateValue;
        private readonly LocalDatePattern _datePattern = LocalDatePattern.CreateWithInvariantCulture("yyyy:MM:dd");

        public LocalDateType()
            : base(SqlTypeFactory.Date)
        {
        }

        public override string Name
        {
            get { return "LocalDate"; }
        }

        public override object Get(IDataReader rs, int index)
        {
            try
            {
                var dbValue = Convert.ToDateTime(rs[index]);
                return LocalDateTime.FromDateTime(dbValue).Date;
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
            get { return typeof(LocalDate); }
        }

        public override void Set(IDbCommand st, object value, int index)
        {
            var parameter = ((SqlParameter)st.Parameters[index]);
            var localDate = (LocalDate)value;
            if (localDate < customBaseDate)
            {
                //parameter.Value = DBNull.Value;
            }
            //else
            {
                parameter.DbType = DbType.Date;
                parameter.Value = new DateTime(localDate.Year, localDate.Month, localDate.Day);
            }
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
            return _datePattern.Format((LocalDate)val);
        }

        public object StringToObject(string xml)
        {
            return string.IsNullOrEmpty(xml) ? null : FromStringValue(xml);
        }

        public override object FromStringValue(string xml)
        {
            return _datePattern.Parse(xml).Value;
        }

        public override Type PrimitiveClass
        {
            get { return typeof(LocalDate); }
        }

        public override object DefaultValue
        {
            get { return customBaseDate; }
        }

        public override string ObjectToSQLString(object value, Dialect dialect)
        {
            return "'" + _datePattern.Format((LocalDate)value) + "'";
        }

        public void SetParameterValues(IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return;
            }
            string value;
            if (parameters.TryGetValue(BaseValueParameterName, out value))
            {
                customBaseDate = _datePattern.Parse(value).Value;
            }
        }
    }
}
