using System;

namespace Cohort.Networking.PhotonKeys {

    public static class Keys{
        
        /// <summary>
        /// Not 100% tested.
        /// </summary>
        /// <param name="key">Key in the photon props</param>
        /// <param name="isRoomProp">Room or player property.</param>
        /// <returns></returns>
        public static string Decode(string key, bool isRoomProp)
        {
            string[] keys = key.Split('_');

            if (!int.TryParse(keys[0], out int keyId)) {
                return key;
            }

            if (isRoomProp) {
                Room r = (Room) keyId;
                string decode = $"{r}";
                
                int i = 1;
                switch (r) {
                    case Room.Game:
                        decode += DecodeGameKey(keys[i]);
                        i++;
                        break;
                }
                
                for (; i < keys.Length; i++) {
                    decode += $"_{keys[i]}";
                }
                return decode;
            }
            else {
                Player p = (Player) keyId;
                return p.ToString();
            }
        }

        private static string DecodeGameKey(string data) {
            return $"Game_{(Game)int.Parse(data)}";
        }
        
        public static string Get<T>(T enumValue) where T : Enum
        {
            return ((int)(object)enumValue).ToString();
        }
        
        public static string GetUUID<T>(T enumValue, string uuid) where T : Enum
        {
            return Concatenate(Get(enumValue), uuid);
        }

        public static string Concatenate(params Enum[] args) {
            if (args.Length == 1)
                return Get(args[0]);
            
            string output = "";
            for (int i = 0; i < args.Length; i++) {
                output += $"{Get(args[i])}_";
            }

            //cut of the last underscore
            return output.Substring(0, output.Length - 1);
        }

        public static string Concatenate(params string[] args)
        {
            if (args.Length == 1)
                return args[0];
            
            string output = "";
            for (int i = 0; i < args.Length; i++) {
                output += $"{args[i]}_";
            }

            //cut of the last underscore
            return output.Substring(0, output.Length - 1);
        }
        
        //id tables
        //TODO_COHORT: new spreadsheet
        //https://docs.google.com/spreadsheets/d/1FhNdohaAcMJwWR2BZ_Pd6ettCdQ3lkXTFYVsIe8bPlc/edit#gid=241574882
        
        public enum Player {
            Name = 1,
            Avatar = 2,
            Jump = 3,
            Position = 4,
            Rotation = 5,
            AvatarVisible = 6,
            LocomotionState = 7,
            Emote = 8,
            EmoteExpire = 9,
            JumpExpire = 10,
        }

        public enum Room
        {
            Interactable = 0,
            TimeLord = 1,
            RefTime = 2,
            Game = 3,
            Scene = 4,
        }

        public enum Game {
            Actor = 0,
            Definition = 1,
            PlayerReady = 2,
            Session = 3,
            Index = 4,
        }
    }
}