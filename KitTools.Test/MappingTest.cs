using KitTools.Test.Models;
using MT.KitTools.Mapper;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace KitTools.Test
{
    public class MappingTest
    {
        byte[] avatarBytes;
        UserDTO userDto = new UserDTO
        {
            NA = "Marvel => 20",
            Address = "123"
        };
        User user = new User
        {
            Name = "Marvel",
            Age = 20,
            Address = "123",
            IDCard = "xxxx"
        };
        [SetUp]
        public void Setup()
        {
            using (FileStream fileStream = File.OpenRead(@"E:\Documents\Desktop\avatar.png"))
            {
                long length = fileStream.Length;
                avatarBytes = new byte[length];
                fileStream.Read(avatarBytes, 0, (int)length);
            }

            Mapper.Default.Configuration(config =>
            {
                config.StringComparison = System.StringComparison.OrdinalIgnoreCase | System.StringComparison.CurrentCulture;
                config.EnablePrefixMatch("TEST_");
            });
            Mapper.Default.CreateMap<User, UserDTO>(profile =>
            {
                profile.Mapping((u, ut) =>
                {
                    ut.NA = $"{u.Name} => {u.Age}";
                });
            });
            userDto.Avatar = avatarBytes;
            user.Avatar = avatarBytes;


        }

        [Test]
        public void AutoMap()
        {
            var ud = Mapper.Map<User, UserDTO>(user);
            Assert.IsTrue(user.Address == ud.Address);
            Assert.IsTrue(user.Avatar.Length == ud.Avatar.Length);
        }

        [Test]
        public void RuleMap()
        {
            var ud = Mapper.Map<User, UserDTO>(user);
            Assert.IsTrue(ud.NA == $"{user.Name} => {user.Age}");
        }

        [Test]
        public void MapToDictionaryObject()
        {
            var u = Mapper.Map<User, IDictionary<string, object>>(user);
            Assert.IsTrue(u.Count == 5);
            Assert.IsTrue(u["Name"].ToString() == "Marvel");
        }

        [Test]
        public void MapToDictionaryInt()
        {
            var u = Mapper.Map<User, IDictionary<string, int>>(user);
            Assert.IsTrue(u.Count == 1);
            Assert.IsTrue(u["Age"] == 20);
        }

        [Test]
        public void MapFrom()
        {
            var u = new User();
            u.Map(user);
        }

        //[Test]
        //public void IEnumerableTest() {
        //    IList<User> list = new List<User> { user };
        //    //var ud = user.Map<User, UserDTO>();
        //    var userDTOs = Mapper.Map<User, UserDTO>(list);
        //    Assert.Pass();
        //}
    }
}