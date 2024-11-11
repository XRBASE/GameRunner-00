using System;

namespace Cohort.Networking.PhotonKeys {

    public static class Keys {

        public const char SEPARATOR = '_';
        
        /// <summary>
        /// Not 100% tested.
        /// </summary>
        /// <param name="key">Key in the photon props</param>
        /// <param name="isRoomProp">Room or player property.</param>
        /// <returns></returns>
        public static string Decode(string key, bool isRoomProp)
        {
            string[] keys = key.Split(SEPARATOR);

            if (!int.TryParse(keys[0], out int keyId)) {
                return key;
            }

            if (isRoomProp) {
                Room r = (Room) keyId;
                string decode = $"{r}";
                
                int i = 1;
                switch (r) {
                    case Room.Activity:
                        decode += DecodeGameKey(keys[i]);
                        i++;
                        break;
                }
                
                for (; i < keys.Length; i++) {
                    decode += $"{SEPARATOR}{keys[i]}";
                }
                return decode;
            }
            else {
                Player p = (Player) keyId;
                return p.ToString();
            }
        }

        private static string DecodeGameKey(string data) {
            return $"Game{SEPARATOR}{(Activity)int.Parse(data)}";
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
                output += Get(args[i]) + SEPARATOR;
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
                output += args[i] + SEPARATOR;
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
            Time = 1,
            RefTime = 2,
            Activity = 3,
            Scene = 4,
            Learning = 5,
        }

        public enum Activity {
            Definition = 1,
            PlayerReady = 2,
            Session = 3,
            Score = 4,
        }

        public enum Learning {
            Index = 0,
            Actor = 1,
            State = 2,
        }

        public enum Time {
            TimeLord = 0,
            TimerState = 1,
            TimerTime = 2,
        }
    }
}