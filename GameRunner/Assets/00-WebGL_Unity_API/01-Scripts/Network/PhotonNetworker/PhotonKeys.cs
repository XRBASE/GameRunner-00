using System;

namespace Cohort.Ravel.PhotonKeys {

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
                
                if (r == Room.File) {
                    return DecodeFileKey(keys);
                }

                if (r == Room.Moderation) {
                    DecodeModerationKey(keys);
                }

                if (r == Room.PortalData) {
                    return DecodePortalKey(keys);
                }

                if (r == Room.Game) {
                    return DecodeGameKey(keys);
                }

                /*if (r == Room.Trigger || r == Room.Poll || r == Room.Viewpoint || r == Room.Chair || r == Room.PlayableState) {
                    return DecodeTypeIdKey(r, keys);
                }*/

                string decode = $"{r}";
                for (int i = 1; i < keys.Length; i++) {
                    decode += $"_{keys[i]}";
                }
                return decode;
            }
            else {
                Player p = (Player) keyId;

                if (p == Player.BadgeVisuals) {
                    return DecodeBadgeVisualsKey(keys);
                }
                
                return p.ToString();
            }
        }

        private static string DecodeFileKey(string[] keys)
        {
            string decoded = "File_";
            int type = int.Parse(keys[1]);
            
            switch (type) {
                case 0: 
                    decoded += "None";
                    break;
                case 1:
                    decoded += "Screen";
                    break;
                case 2:
                    decoded += "Model";
                    break;
                default:
                    decoded += "UndefinedType";
                    break;
            }
            
            decoded += $"_{int.Parse(keys[2])}";
            if (keys.Length >= 3) {
                for (int i = 3; i < keys.Length; i++) {
                    decoded += (ObjectProperties)(int.Parse(keys[i]));
                }
            }
            
            return decoded;
        }

        private static string DecodePortalKey(string[] keys)
        {
            string decoded = $"PortalController_{keys[1]}";
            
            if (keys.Length >= 2) {
                for (int i = 2; i < keys.Length; i++) {
                    if (int.TryParse(keys[i], out int keyI)) {
                        decoded += $"_{(ObjectProperties)(keyI)}";
                    }
                    else {
                        decoded += $"_{keyI}";
                    }
                }
            }
            
            return decoded;
        }
        
        private static string DecodeGameKey(string[] keys)
        {
            //game_id_modifier
            string decoded = $"Game_{int.Parse(keys[1])}_{(Game)int.Parse(keys[2])}";
            
            if (keys.Length > 3) {
                for (int i = 3; i < keys.Length; i++) {
                    decoded += $"_{keys[i]}";
                }
            }
            
            return decoded;
        }

        private static string DecodeModerationKey(string[] keys)
        {
            string decoded = "Moderation_";
            decoded += (ModerationSetting) int.Parse(keys[1]);
            return decoded;
        }

        private static string DecodeBadgeVisualsKey(string[] keys) {
            string decoded = $"BadgeVisuals_{(BadgeVisual)int.Parse(keys[1])}";
            
            if (keys.Length > 2) {
                for (int i = 2; i < keys.Length; i++) {
                    decoded += $"_{keys[i]}";
                }
            }
            return decoded;
        }

        private static string DecodeTypeIdKey<T>(T type, string[] keys) where T : Enum
        {
            return  $"{type}_{keys[1]}";
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
        //https://docs.google.com/spreadsheets/d/1FhNdohaAcMJwWR2BZ_Pd6ettCdQ3lkXTFYVsIe8bPlc/edit#gid=241574882
        
        public enum Player {
            Uuid = 0,
            Name = 1,
            Avatar = 2,
            Jump = 3,
            Muted = 4, //not used yet, but set up so present panel can already use this key
            Position = 5,
            Rotation = 6,
            AvatarVisible = 7,
            MetaData = 10,
            LocomotionState =11,
            InGame = 12,
            BadgeVisuals = 13,
            Emote = 15,
            EmoteExpire = 16,
            JumpExpire = 17,
        }

        public enum Room
        {
            PortalRefresh = 0,
            IDList = 1, //IDList key is concatted with the id of the matching ID type, see IDManager class.
            File = 2,
            PortalData = 3,
            Trigger = 4,
            PollIdList = 5,
            Poll = 6,
            RoleRefresh = 7,
            Moderation = 8,
            ViewpointCount = 9,
            Viewpoint = 10,
            Chair = 11,
            TimeLord = 13,
            RefTime = 14,
            PlayableState = 15,
            Game = 16,
            MetaData = 17,
            Questionnaire = 18,
            Quiz = 19,
            PlayerGroup = 20,
            PlayerInfo = 21,
        }

        public enum Game
        {
            Scores = 0,
            Names,
            PrefabId,
            Trigger = 3, //steps in the timeline of the game that are completed
            EvalScore = 4, //new score that has not yet been evaluated yet
            InGame = 5,
            Session = 6,
            Wheel = 7,
            Progress = 8,
        }

        public enum WheelGame
        {
            Index = 0,
            Options = 1,
        }

        public enum BadgeVisual
        {
            BadgeColor = 0,
            BadgeIcon = 1,
        }

        /// <summary>
        /// Object properties, this includes files, but also portals and other networked objects.
        /// </summary>
        public enum ObjectProperties
        {
            Position = 0,
            Rotation = 1,
            Scale = 2,
            Page = 3,
            GrabbedBy = 4,
            Locked = 5,
            AgoraUid = 6,
            OwnerUserId = 7,
            LiveStreamChannel = 8,
            VideoState = 9,
            VideoLoop = 10,
        }

        public enum ModerationSetting
        {
            MovementDisabled =1,
            ShowNameTags = 2,
            DanceAll = 3,
        }

        public enum QuestionnaireType
        {
            String = 0,
            GameObject = 1,
        }

        public enum QuizProperties
        {
            QuestionId = 0,
            OpenQuestionInput = 1,
            MultiQuestionInput = 2,
            State = 3,
            Paused = 4,
        }
    }
}