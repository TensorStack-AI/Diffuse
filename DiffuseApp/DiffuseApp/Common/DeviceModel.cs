using Diffuse.Services;
using TensorStack.Common;
using TensorStack.Python.Common;

namespace Diffuse.Common
{
    public record DeviceModel : Device
    {
        public DeviceModel() { }
        public DeviceModel(Device options, GPUDevice gpuDevice) : base(options)
        {
            PCIBusId = gpuDevice.PCIBusId;
            IsLoraSupported = options.Vendor == VendorType.Nvidia || options.Vendor == VendorType.AMD;
            DefaultDataType = options.Vendor == VendorType.AMD ? DataType.Int8 : DataType.Float8;
            DataTypes = options.Vendor == VendorType.AMD
            ? [DataType.Bfloat16, DataType.Float16, DataType.Int8]
            : [DataType.Bfloat16, DataType.Float16, DataType.Float8, DataType.Int8];
        }

        public DataType[] DataTypes { get; init; }
        public bool IsLoraSupported { get; init; }
        public int PCIBusId { get; init; }
        public DataType DefaultDataType { get; init; }
    }
}
