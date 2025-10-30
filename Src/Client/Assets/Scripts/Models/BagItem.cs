using System.Runtime.InteropServices;

namespace Models
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BagItem
    {
        /// <summary>
        /// ID==0是代表空位置
        /// </summary>
        public ushort ItemId;//物品ID
        public ushort Count;//物品数量

        public static BagItem zero = new BagItem { ItemId = 0, Count = 0 };

        public BagItem(int itemId, int count)
        {
            this.ItemId = (ushort)itemId;
            this.Count = (ushort)count;
        }

        public static bool operator ==(BagItem lhs, BagItem rhs)
        {
            return lhs.ItemId == rhs.ItemId && lhs.Count == rhs.Count;
        }

        public static bool operator !=(BagItem lhs, BagItem rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// <para>Returns true if the objects are equal.</para>
        /// </summary>
        /// <param name="other"></param>
        public override bool Equals(object other)
        {
            if (other is BagItem)
            {
                return Equals((BagItem)other);
            }
            return false;
        }

        public bool Equals(BagItem other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return ItemId.GetHashCode() ^ (Count.GetHashCode() << 2);
        }
    }
}
