using KitTools.Test.Models;
using MT.KitTools.Mapper;
using NUnit.Framework;
using System.IO;

namespace KitTools.Test {
    public class MappingTest {
        byte[] avatarBytes;
        UserDTO userDto = new UserDTO {
            NA = "Marvel => 20",
            Address = "123"
        };
        User user = new User {
            Name = "Marvel",
            Age = 20,
            Address = "123",
            IDCard = "xxxx"
        };
        [SetUp]
        public void Setup() {
            using (FileStream fileStream = File.OpenRead(@"E:\Documents\Desktop\avatar.png")) {
                long length = fileStream.Length;
                avatarBytes = new byte[length];
                fileStream.Read(avatarBytes, 0, (int)length);
            }

            Mapper.Default.Configuration(config => {
                config.StringComparison = System.StringComparison.OrdinalIgnoreCase | System.StringComparison.CurrentCulture;
                config.EnablePrefixMatch("TEST_");
            });

            Mapper.Default.CreateMap<User, UserDTO>(profile => {
                profile.Mapping((u, ut) => {
                    ut.NA = $"{u.Name} => {u.Age}";
                });
            });
            userDto.Avatar = avatarBytes;
            user.Avatar = avatarBytes;


        }

        [Test]
        public void AutoMap() {
            var ud = user.Map<User, UserDTO>();
            Assert.IsTrue(user.Address == ud.Address);
            Assert.IsTrue(user.Avatar.Length == ud.Avatar.Length);
        }

        [Test]
        public void RuleMap() {
            var ud = user.Map<User, UserDTO>();
            Assert.IsTrue(ud.NA == $"{user.Name} => {user.Age}");
        }

        //[Test]
        //public void BackwardMap() {
        //    var u = Mapper.Map<UserDTO, User>(userDto);
        //    Assert.IsTrue(u.Name == "Marvel");
        //    Assert.IsTrue(u.Age == 20);
        //}

        [Test]
        public void IEnumerableTest() {
            Assert.Pass();
        }


    }
}