using TensorStack.Common;
using TensorStack.Python.Common;

namespace Diffuse.Common
{
    public record DeviceModel : Device
    {
        public DeviceModel() { }
        public DeviceModel(Device options) : base(options)
        {
            IsLoraSupported = options.Vendor != VendorType.AMD;
            DataTypes = options.Vendor == VendorType.AMD
            ? [DataType.Bfloat16, DataType.Float16, DataType.Int8]
            : [DataType.Bfloat16, DataType.Float16, DataType.Float8, DataType.Int8];
        }

        public DataType[] DataTypes { get; init; }
        public bool IsLoraSupported { get; init; }
    }
}
