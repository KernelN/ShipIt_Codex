namespace Universal.FileManaging.Types
{
    [System.Serializable]
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vec3(Vec3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }
        public static implicit operator UnityEngine.Vector3(Vec3 vec)
        {
            return new UnityEngine.Vector3(vec.x, vec.y, vec.z);
        }
        public static implicit operator Vec3(UnityEngine.Vector3 vec)
        {
            return new Vec3(vec.x, vec.y, vec.z);
        }
        public static implicit operator UnityEngine.Vector3Int(Vec3 vec)
        {
            return new UnityEngine.Vector3Int((int)vec.x, (int)vec.y, (int)vec.z);
        }
        public static implicit operator Vec3(UnityEngine.Vector3Int vec)
        {
            return new Vec3(vec.x, vec.y, vec.z);
        }
    }
}
