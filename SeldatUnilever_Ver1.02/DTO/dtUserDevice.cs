namespace SeldatUnilever_Ver1._02.DTO
{
    public class dtUserDevice : userModel
    {
        private int pUserDeviceId;
        private int pUserId;
        private int pDeviceId;

        public int userDeviceId { get => pUserDeviceId; set => pUserDeviceId = value; }
        public int userId { get => pUserId; set => pUserId = value; }
        public int deviceId { get => pDeviceId; set => pDeviceId = value; }
    }
}
