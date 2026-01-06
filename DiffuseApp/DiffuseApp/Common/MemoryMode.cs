namespace Diffuse.Common
{
    public enum MemoryMode
    {
        Auto = 0,

        // IsFullOffloadEnabled: true, IsModelOffloadEnabled: false, IsVaeSlicingEnabled: true, IsVaeTilingEnabled: true
        Minimum = 1,

        // IsFullOffloadEnabled: false, IsModelOffloadEnabled: true, IsVaeSlicingEnabled: true, IsVaeTilingEnabled: true
        Medium = 3,

        // IsFullOffloadEnabled: false, IsModelOffloadEnabled: true, IsVaeSlicingEnabled: false, IsVaeTilingEnabled: false
        High = 4,

        // IsFullOffloadEnabled: false, IsModelOffloadEnabled: false, IsVaeSlicingEnabled: false, IsVaeTilingEnabled: false
        Maximum = 5
    }
}
