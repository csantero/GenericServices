﻿#region licence
// The MIT License (MIT)
// 
// Filename: Perf08PostsViaDetailDtoAsync.cs
// Date Created: 2014/06/17
// 
// Copyright (c) 2014 Jon Smith (www.selectiveanalytics.com & www.thereformedprogrammer.net)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System;
using System.Data.Entity;
using System.Linq;
using GenericServices.ServicesAsync.Concrete;
using NUnit.Framework;
using Tests.DataClasses;
using Tests.DataClasses.Concrete;
using Tests.DTOs.Concrete;
using Tests.Helpers;

namespace Tests.UnitTests.Group80Performance
{
    class Perf08PostsViaDetailDtoAsync
    {

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            new DetailPostDtoAsync().CacheSetup();
        }

        [SetUp]
        public void SetUp()
        {
            using (var db = new SampleWebAppDb())
            {
                DataLayerInitialise.InitialiseThis();
                var filepath = TestFileHelpers.GetTestFileFilePath("DbContentSimple.xml");
                DataLayerInitialise.ResetDatabaseToTestData(db, filepath);
            }
        }


        [Test]
        public async void Perf03DetailPostOk()
        {
            int postId;
            using (var db = new SampleWebAppDb())
                postId = db.Posts.Include(x => x.Tags).AsNoTracking().First().PostId;

            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var service = new DetailServiceAsync<Post, DetailPostDtoAsync>(db);

                //ATTEMPT
                var status = await service.GetDetailAsync(postId);
                status.Result.LogSpecificName("End");

                //VERIFY
                foreach (var log in status.Result.LogOfCalls) { Console.WriteLine(log); }
            }
        }

        [Test]
        public async void Perf10CreateSetupServiceOk()
        {

            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var service = new CreateSetupServiceAsync<Post, DetailPostDtoAsync>(db);

                //ATTEMPT
                var dto = await service.GetDtoAsync();
                dto.LogSpecificName("End");

                //VERIFY
                foreach (var log in dto.LogOfCalls) { Console.WriteLine(log); }
            }
        }


        [Test]
        public async void Perf11CreatePostOk()
        {

            using (var db = new SampleWebAppDb())
            {
                //SETUP

                var service = new CreateServiceAsync<Post, DetailPostDtoAsync>(db);
                var setupService = new CreateSetupServiceAsync<Post, DetailPostDtoAsync>(db);

                //ATTEMPT
                var dto = await setupService.GetDtoAsync();
                dto.Title = Guid.NewGuid().ToString();
                dto.Content = "something to fill it as can't be empty";
                dto.Bloggers.SelectedValue = db.Blogs.First().BlogId.ToString("D");
                dto.UserChosenTags.FinalSelection = db.Tags.Take(2).ToList().Select(x => x.TagId.ToString("D")).ToArray();
                var status = await service.CreateAsync(dto);
                dto.LogSpecificName("End");

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                foreach (var log in dto.LogOfCalls) { Console.WriteLine(log); }
            }
        }



        [Test]
        public async void Perf16UpdateSetupServiceOk()
        {
            int postId;
            using (var db = new SampleWebAppDb())
                postId = db.Posts.Include(x => x.Tags).AsNoTracking().First().PostId;

            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var setupService = new UpdateSetupServiceAsync<Post, DetailPostDtoAsync>(db);

                //ATTEMPT
                var status = await setupService.GetOriginalAsync(postId);
                status.Result.LogSpecificName("End");

                //VERIFY
                foreach (var log in status.Result.LogOfCalls) { Console.WriteLine(log); }
            }
        }



        [Test]
        public async void Perf22UpdatePostAddTagOk()
        {
            int postId;
            using (var db = new SampleWebAppDb())
                postId = db.Posts.Include(x => x.Tags).AsNoTracking().First().PostId;

            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var setupService = new UpdateSetupServiceAsync<Post, DetailPostDtoAsync>(db);
                var updateService = new UpdateServiceAsync<Post, DetailPostDtoAsync>(db);

                //ATTEMPT
                var setupStatus = await setupService.GetOriginalAsync(postId);
                setupStatus.Result.Title = Guid.NewGuid().ToString();
                setupStatus.Result.Bloggers.SelectedValue = db.Blogs.First().BlogId.ToString("D");
                setupStatus.Result.UserChosenTags.FinalSelection = db.Tags.Take(3).ToList().Select(x => x.TagId.ToString("D")).ToArray();
                var status = await updateService.UpdateAsync(setupStatus.Result);
                setupStatus.Result.LogSpecificName("End");

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                foreach (var log in setupStatus.Result.LogOfCalls) { Console.WriteLine(log); }
            }
        }

    }
}
