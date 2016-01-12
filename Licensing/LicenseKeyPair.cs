using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licensing
{
    [Serializable]
    public struct LicenseKeyPair
    {
        public string name;

        public byte[] publicKey;
        public byte[] privateKey;
    }

}
