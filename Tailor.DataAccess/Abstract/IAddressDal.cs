using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Abstract
{
    public interface IAddressDal : IGenericDal<Address>
    {
        // Kullanıcıya ait adresleri getiren özel metot
        List<Address> GetAddressesByUserId(int userId);
    }
}