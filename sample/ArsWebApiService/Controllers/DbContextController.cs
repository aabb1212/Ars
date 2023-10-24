using Ars.Common.Core.AspNetCore.OutputDtos;
using Ars.Common.Core.Configs;
using Ars.Common.Core.IDependency;
using Ars.Common.Core.Uow;
using Ars.Common.Core.Uow.Attributes;
using Ars.Common.EFCore.AdoNet;
using Ars.Common.EFCore.Extension;
using Ars.Common.EFCore.Repository;
using ArsWebApiService;
using ArsWebApiService.Model;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using MyApiWithIdentityServer4.Dtos;
using MyApiWithIdentityServer4.Model;
using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Transactions;

namespace MyApiWithIdentityServer4.Controllers
{
    /// <summary>
    /// dbcontext test controller
    /// </summary>
    public class DbContextController : MyControllerBase
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DbContextController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly ITestScopeService _testScopeService;
        private readonly IArsIdentityClientConfiguration _clientConfiguration;
        //private readonly MyDbContext myDbContext;
        private readonly IUnitOfWork _unitOfWork;

        public DbContextController(ILogger<DbContextController> logger,
            MyDbContext myDbContext,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            //ITestScopeService testScopeService,
            IArsIdentityClientConfiguration arsIdentityClientConfiguration,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            //this.myDbContext = myDbContext;
            _httpContextAccessor = httpContextAccessor;
            //_testScopeService = testScopeService;
            _clientConfiguration = arsIdentityClientConfiguration;
            _unitOfWork = unitOfWork;
        }

        [Autowired]
        public IRepository<Student, Guid> Repo { get; set; }

        [Autowired]
        public IRepository<StudentMsSql, Guid> Repo1 { get; set; }

        [Autowired]
        public IRepository<ClassRoom, Guid> ClassRepo { get; set; }

        [Autowired]
        public IDbExecuter<MyDbContext> DbExecuter { get; set; }

        [Autowired]
        protected IRepository<AppVersion> RepoApp { get; set; }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };


        [HttpGet(Name = "GetWeatherForecast")]
        //[Authorize]
        public IEnumerable<WeatherForecast> Get()
        {
            TestService.Test();

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        #region DbContext Without Transaction
        [HttpPost(nameof(ActionWithOutTransaction))]
        public async Task ActionWithOutTransaction()
        {
            Guid id = Guid.NewGuid();

            await MyDbContext.Students.AddAsync(new Model.Student
            {
                Id = id,
                EnrollmentDate = DateTime.Now,
                FirstMidName = "Boo",
                LastName = "Yang",
                Enrollments = new[]
                {
                    new Model.Enrollment
                    {
                        EnrollmentID = 3,
                        CourseID = 3,
                        StudentID = id,
                        Grade = Model.Grade.A,
                        Course = new Model.Course
                        {
                            CourseID = 3,
                            Title = "2023.03.01.001",
                            Credits = 100.11m,
                            Name = "2023.03.01.001"
                        }
                    }
                }
            });

            await MyDbContext.SaveChangesAsync();
        }

        [HttpGet(nameof(Query))]
        public async Task<IActionResult> Query()
        {
            var m = await MyDbContext.Students.FirstOrDefaultAsync(r => r.LastName.Equals("8899"));
            var n = await MyDbContext.Students.Include(r => r.Enrollments).FirstOrDefaultAsync(r => r.LastName.Equals("Yang"));
            var o = await MyDbContext.Students.Include(r => r.Enrollments).ThenInclude(r => r.Course).FirstOrDefaultAsync(r => r.LastName.Equals("Yang"));

            return Ok((m, n, o));
        }

        [Authorize]
        [HttpPost(nameof(ModifyWithOutTransaction))]
        public async Task ModifyWithOutTransaction()
        {
            var info = await MyDbContext.Students.FirstOrDefaultAsync();
            info.LastName = "boo" + new Random().Next(20);

            await MyDbContext.SaveChangesAsync();
        }

        [Authorize]
        [HttpPost(nameof(DeleteWithOutTransaction))]
        public async Task DeleteWithOutTransaction()
        {
            var info = await MyDbContext.Students.FirstOrDefaultAsync();
            MyDbContext.Students.Remove(info);

            await MyDbContext.SaveChangesAsync();
        }
        #endregion

        #region DbContext with Transaction

        [HttpPost(nameof(TestUowDefault))]
        public async Task TestUowDefault()
        {
            MyDbContext _dbContext = await UnitOfWorkManager.Current.GetDbContextAsync<MyDbContext>();
            await _dbContext.Students.AddAsync(new Model.Student
            {
                LastName = "TestUowDefault11",
                FirstMidName = "TestUowDefault11",
                EnrollmentDate = DateTime.Now,
            });

            string sql = @"insert into Students(Id,LastName,FirstMidName,EnrollmentDate,TenantId,CreationUserId,IsDeleted) " +
                "values(@Id,@LastName,@FirstMidName,@EnrollmentDate,@TenantId,@CreationUserId,@IsDeleted)";
            MySqlParameter[] sqlParameters =
            {
                new MySqlParameter("@Id",Guid.NewGuid()),
                new MySqlParameter("@LastName",8899),
                new MySqlParameter("@FirstMidName","aabb121211"),
                new MySqlParameter("@EnrollmentDate",DateTime.Now),
                new MySqlParameter("@TenantId",1),
                new MySqlParameter("@CreationUserId",1),
                new MySqlParameter("@IsDeleted",false),
            };

            DbExecuter.BeginWithEFCoreTransaction(UnitOfWorkManager.Current!);
            var count = await DbExecuter.ExecuteNonQuery(sql, sqlParameters);

            string updatesql = $"update Students set LastName = @LastName where FirstMidName = @FirstMidName";
            MySqlParameter[] upsqlParameters =
            {
                 new MySqlParameter("@LastName",889999),
                 new MySqlParameter("@FirstMidName","aabb121211"),
            };
            count = await DbExecuter.ExecuteNonQuery(updatesql, upsqlParameters);
        }

        [HttpPost(nameof(TestUowRequired))]
        public async Task TestUowRequired()
        {
            using var scope1 = UnitOfWorkManager.Begin(TransactionScopeOption.Required);
            MyDbContext _dbContext = await UnitOfWorkManager.Current.GetDbContextAsync<MyDbContext>();
            await _dbContext.Students.AddAsync(new Model.Student
            {
                LastName = "TestUowRequired",
                FirstMidName = "TestUowRequired",
                EnrollmentDate = DateTime.UtcNow,
            });
            await scope1.CompleteAsync();
        }

        [HttpPost(nameof(TestUowRequiredNew))]
        public async Task TestUowRequiredNew()
        {
            using var scope1 = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew);
            MyDbContext _dbContext = await UnitOfWorkManager.Current.GetDbContextAsync<MyDbContext>();
            await _dbContext.Students.AddAsync(new Model.Student
            {
                LastName = "TestUowNewRequired",
                FirstMidName = "TestUowNewRequired",
                EnrollmentDate = DateTime.Now,
            });

            string sql = @"insert into Students(Id,LastName,FirstMidName,EnrollmentDate,TenantId,CreationUserId,IsDeleted) " +
                "values(@Id,@LastName,@FirstMidName,@EnrollmentDate,@TenantId,@CreationUserId,@IsDeleted)";
            MySqlParameter[] sqlParameters =
            {
                new MySqlParameter("@Id",Guid.NewGuid()),
                new MySqlParameter("@LastName",8899),
                new MySqlParameter("@FirstMidName","aabb121212"),
                new MySqlParameter("@EnrollmentDate",DateTime.Now),
                new MySqlParameter("@TenantId",1),
                new MySqlParameter("@CreationUserId",1),
                new MySqlParameter("@IsDeleted",false),
            };

            DbExecuter.BeginWithEFCoreTransaction(UnitOfWorkManager.Current!);
            var count = await DbExecuter.ExecuteNonQuery(sql, sqlParameters);

            string updatesql = $"update Students set LastName = @LastName where FirstMidName = @FirstMidName";
            MySqlParameter[] upsqlParameters =
            {
                 new MySqlParameter("@LastName",889999),
                 new MySqlParameter("@FirstMidName","aabb121212"),
            };
            count = await DbExecuter.ExecuteNonQuery(updatesql, upsqlParameters);

            await scope1.CompleteAsync();
        }

        [HttpPost(nameof(TestSuppress))]
        public async Task TestSuppress()
        {
            using (var scope = UnitOfWorkManager.Begin(TransactionScopeOption.Suppress))
            {
                MyDbContext _dbContext = await UnitOfWorkManager.Current.GetDbContextAsync<MyDbContext>();
                await _dbContext.Students.AddAsync(new Model.Student
                {
                    LastName = "Suppress",
                    FirstMidName = "Suppress",
                    EnrollmentDate = DateTime.UtcNow,
                });
                await scope.CompleteAsync();
            }
        }

        [HttpPost(nameof(TestSuppressInnerRequired))]
        public async Task TestSuppressInnerRequired()
        {
            using var scope0 = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew);
            UnitOfWorkManager.Current.Completed += (sender, args) =>
            {

            };
            MyDbContext dbContext0 = await UnitOfWorkManager.Current.GetDbContextAsync<MyDbContext>();
            await dbContext0.Students.AddAsync(new Model.Student
            {
                LastName = "RequiresNew",
                FirstMidName = "RequiresNew",
                EnrollmentDate = DateTime.UtcNow,
            });
            await scope0.CompleteAsync(); //�ύ����

            using (var scope = UnitOfWorkManager.Begin(TransactionScopeOption.Suppress))
            {
                UnitOfWorkManager.Current.Completed += (sender, args) =>
                {

                };
                MyDbContext _dbContext = await UnitOfWorkManager.Current.GetDbContextAsync<MyDbContext>();
                await _dbContext.Students.AddAsync(new Model.Student
                {
                    LastName = "Suppress.Out.001",
                    FirstMidName = "Suppress.Out.001",
                    EnrollmentDate = DateTime.UtcNow,
                });

                using var scope1 = UnitOfWorkManager.Begin(TransactionScopeOption.Required);
                UnitOfWorkManager.Current.Completed += (sender, args) =>
                {

                };
                MyDbContext dbContext1 = await UnitOfWorkManager.Current.GetDbContextAsync<MyDbContext>();
                await dbContext1.Students.AddAsync(new Model.Student
                {
                    LastName = "Suppress.Required.Inner.001",
                    FirstMidName = "Suppress.Required.Inner",
                    EnrollmentDate = DateTime.UtcNow,
                });
                await scope1.CompleteAsync(); //ֱ��SaveChangesAsync��û���ύ����
                await scope.CompleteAsync();//�ύ����
            }

            using (var scope = UnitOfWorkManager.Begin(TransactionScopeOption.Suppress))
            {
                UnitOfWorkManager.Current.Completed += (sender, args) =>
                {

                };
                MyDbContext _dbContext = await UnitOfWorkManager.Current.GetDbContextAsync<MyDbContext>();
                await _dbContext.Students.AddAsync(new Model.Student
                {
                    LastName = "Suppress.Out.002",
                    FirstMidName = "Suppress.Out.002",
                    EnrollmentDate = DateTime.UtcNow,
                });

                await scope.CompleteAsync();//�ύ����
            }

            using (var scope = UnitOfWorkManager.Begin(TransactionScopeOption.Suppress))
            {
                MyDbContext _dbContext = await UnitOfWorkManager.Current.GetDbContextAsync<MyDbContext>();
                await _dbContext.Students.AddAsync(new Model.Student
                {
                    LastName = "Suppress.Out.003",
                    FirstMidName = "Suppress.Out.003",
                    EnrollmentDate = DateTime.UtcNow,
                });

                await _dbContext.SaveChangesAsync();
                await scope.CompleteAsync();//�ύ����
            }
        }

        [UnitOfWork(IsDisabled = true)]
        [HttpPost(nameof(TestUowWithDispose))]
        public async Task TestUowWithDispose()
        {
            using var scope = UnitOfWorkManager.Begin(TransactionScopeOption.Required);
            MyDbContext _dbContext = await UnitOfWorkManager.Current.GetDbContextAsync<MyDbContext>();
            await _dbContext.Students.AddAsync(new Model.Student
            {
                LastName = "TestUowWithDispose",
                FirstMidName = "TestUowWithDispose",
                EnrollmentDate = DateTime.UtcNow,
            });
            await _dbContext.SaveChangesAsync();
            await scope.CompleteAsync();
        }

        #endregion

        #region IRepository
        
        /// <summary>
        /// ����mysql ���ظ�����������뼶��
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDataByRR() 
        {
            #region ��ζ�����ʹ��һ������update,insert,delete����ͬ���������ݣ���ǰ�����ȡ��������δ�� 
            var query = Repo.GetAll().Where(r => r.LastName == "123");

            var data = await query.ToListAsync();

            // * * * * *
            //��ʱ��һ������insert����������
            // * * * * *

            await Task.Delay(100);

            data = await query.ToListAsync();

            await Task.Delay(100);

            data = await query.ToListAsync();

            #endregion

            #region ��ǰ�����update�������Ὣ��һ������insert������Ҳ��update��

            DbExecuter.BeginWithEFCoreTransaction(UnitOfWorkManager.Current!);
            string updatesql = $"update Students set FirstMidName = @FirstMidName where LastName = @LastName";
            MySqlParameter[] upsqlParameters =
            {
                 new MySqlParameter("@LastName","123"),
                 new MySqlParameter("@FirstMidName","���Ϻ�12"),
            };
            await DbExecuter.ExecuteNonQuery(updatesql, upsqlParameters);

            #endregion

            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var a = await Repo.GetAll().IgnoreQueryFilters().ToListAsync();
            var b = await Repo.GetAllIncluding(r => r.Enrollments).ToListAsync();
            var c = Repo.GetAllList();
            var d = Repo.GetAllList(r => r.Enrollments.Any(t => t.EnrollmentID == 1));
            var e = Repo.FirstOrDefault(r => r.Id == new Guid("8FB45ADF-3F80-45ED-93CB-10A61CE644E9"));

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> InsertWithIdAsync()
        {
            Guid id = Guid.NewGuid();
            var f = await Repo.InsertAsync(new Student
            {
                Id = id,
                EnrollmentDate = DateTime.Now,
                FirstMidName = "7777",
                LastName = "77778",
                Enrollments = new[]
                {
                    new Model.Enrollment
                    {
                        EnrollmentID = 6,
                        CourseID = 6,
                        StudentID = id,
                        Grade = Model.Grade.A,
                        Course = new Model.Course
                        {
                            CourseID = 6,
                            Title = "2023.03.06.002",
                            Credits = 100.11m,
                            Name = "2023.03.06.002"
                        }
                    }
                }
            });

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> InsertWithOutIdAsync()
        {
            var f = await Repo.InsertAsync(new Student
            {
                EnrollmentDate = DateTime.Now,
                FirstMidName = "6666",
                LastName = "6666",
            });

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAsync()
        {
            var e = await Repo.FirstOrDefaultAsync(r => r.Id == new Guid("8FB45ADF-3F80-45ED-93CB-10A61CE644E9"));
            e.LastName = "8888";

            foreach (var en in e.Enrollments)
            {
                en.Grade = Grade.C;
                en.Course.Name = "8888";
            }

            await Repo.UpdateAsync(e);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAsync()
        {
            var h = await Repo.FirstOrDefaultAsync(r => r.Id == new Guid("CAEF9CEF-EBA3-47DA-AAF9-CF2802413F97"));
            await Repo.DeleteAsync(h);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsyncTest()
        {
            var a = await (await Repo.GetAllAsync()).ToListAsync();
            var b = await (await Repo.GetAllIncludingAsync(r => r.Enrollments)).ToListAsync();
            var c = await Repo.GetAllListAsync();
            var d = await Repo.GetAllListAsync(r => r.Enrollments.Any(t => t.EnrollmentID == 1));
            var e = await Repo.FirstOrDefaultAsync(r => r.Id == new Guid("8FB45ADF-3F80-45ED-93CB-10A61CE644E9"));

            return Ok(a);
        }

        [HttpGet]
        [UnitOfWork(IsDisabled=true)]
        public async Task<IActionResult> GetWithOutTransaction()
        {
            try
            {
                await Repo.GetAll().ToListAsync();
            }
            catch (Exception e) 
            {

            }
            return Ok();
        }

        #endregion

        #region ado.net
        [HttpPost]
        public async Task<IActionResult> AdoNetInsert() 
        {
            string sql = @"insert into Students(Id,LastName,FirstMidName,EnrollmentDate,TenantId,CreationUserId,IsDeleted) " +
                "values(@Id,@LastName,@FirstMidName,@EnrollmentDate,@TenantId,@CreationUserId,@IsDeleted)";
            MySqlParameter[] sqlParameters = 
            {
                new MySqlParameter("@Id",Guid.NewGuid()),
                new MySqlParameter("@LastName",123),
                new MySqlParameter("@FirstMidName",223),
                new MySqlParameter("@EnrollmentDate",DateTime.Now),
                new MySqlParameter("@TenantId",1),
                new MySqlParameter("@CreationUserId",1),
                new MySqlParameter("@IsDeleted",false),
            };
            var count = await DbExecuter.ExecuteNonQuery(sql, sqlParameters);
            return Ok(count);
        }

        [HttpPost]
        public async Task<IActionResult> AdoNetInsertWithTransaction()
        {
            string sql = @"insert into Students(Id,LastName,FirstMidName,EnrollmentDate,TenantId,CreationUserId,IsDeleted) " +
                "values(@Id,@LastName,@FirstMidName,@EnrollmentDate,@TenantId,@CreationUserId,@IsDeleted)";
            MySqlParameter[] sqlParameters =
            {
                new MySqlParameter("@Id",Guid.NewGuid()),
                new MySqlParameter("@LastName",8899),
                new MySqlParameter("@FirstMidName","aabb1212"),
                new MySqlParameter("@EnrollmentDate",DateTime.Now),
                new MySqlParameter("@TenantId",1),
                new MySqlParameter("@CreationUserId",1),
                new MySqlParameter("@IsDeleted",false),
            };

            using var scope = await DbExecuter.BeginTransactionAsync();
            var count = await DbExecuter.ExecuteNonQuery(sql, sqlParameters);

            string updatesql = $"update Students set LastName = @LastName where FirstMidName = @FirstMidName";
            MySqlParameter[] upsqlParameters = 
            {
                 new MySqlParameter("@LastName",889999),
                 new MySqlParameter("@FirstMidName","aabb1212"),
            };
            count = await DbExecuter.ExecuteNonQuery(updatesql, upsqlParameters);

            await scope.CommitAsync();
            return Ok(count);
        }

        [HttpPost]
        public async Task<IActionResult> AdoNetUpdate() 
        {
            using var scope = await DbExecuter.BeginTransactionAsync();
            var guids = new Guid[] { new Guid("654d3562-37ad-4ff6-8f93-01f988c75fe1") };
            List<MySqlParameter> sqlParameters = new List<MySqlParameter>
            {
                new MySqlParameter("@LastName","��õĿ��׿�ؼ"),
            };
            StringBuilder ids = new();
            for (var i = 0; i < guids.Count(); i++)
            {
                ids.Append($"@id{i},");
                sqlParameters.Add(new MySqlParameter($"@id{i}", guids[i]));
            }
            string @id = ids.ToString().TrimEnd(',');
            string sql = $"update Students set LastName = @LastName where id in ({@id})";
            var count = await DbExecuter.ExecuteNonQuery(sql, sqlParameters.ToArray());

            var guids1 = new Guid[] { new Guid("654d3562-37ad-4ff6-8f93-01f988c75fe1") };
            List<MySqlParameter> sqlParameters1 = new List<MySqlParameter>();
            StringBuilder ids1 = new();
            for (var i = 0; i < guids1.Count(); i++)
            {
                ids1.Append($"@id{i},");
                sqlParameters1.Add(new MySqlParameter($"@id{i}", guids[i]));
            }
            string @id1 = ids1.ToString().TrimEnd(',');
            string sql1 = $"select * from Students where id in ({@id1})";
            var datas = await DbExecuter.QueryAsync<Student>(sql1, sqlParameters1.ToArray());

            await scope.CommitAsync();

            return Ok((count,datas));
         }

        [HttpPost]
        public async Task<IActionResult> AdoNetDelete() 
        {
            var guids = new Guid[] { new Guid("654d3562-37ad-4ff6-8f93-01f988c75fe1") };
            List<MySqlParameter> sqlParameters = new List<MySqlParameter>();
            StringBuilder ids = new();
            for (var i = 0; i < guids.Count(); i++)
            {
                ids.Append($"@id{i},");
                sqlParameters.Add(new MySqlParameter($"@id{i}", guids[i]));
            }
            string @id = ids.ToString().TrimEnd(',');
            string sql = $"delete from Students where id in ({@id})";
            var count = await DbExecuter.ExecuteNonQuery(sql, sqlParameters.ToArray());
            return Ok(count);
        }

        [HttpGet]
        public async Task<IActionResult> AdoNetQuery() 
        {
            var guids = new Guid[] { new Guid("846f3141-53fa-4d49-8b84-d1213fd1d7e1") };
            List<MySqlParameter> sqlParameters = new List<MySqlParameter>();
            StringBuilder ids = new();
            for (var i = 0; i < guids.Count(); i++)
            {
                ids.Append($"@id{i},");
                sqlParameters.Add(new MySqlParameter($"@id{i}", guids[i]));
            }
            string @id = ids.ToString().TrimEnd(',');
            string sql = $"select * from Students where id in ({@id})";
            var datas = await DbExecuter.QueryAsync<Student>(sql, sqlParameters.ToArray());

            sql = "select count(FirstMidName) as count,FirstMidName from students group by FirstMidName";
            var data2 = await DbExecuter.QueryAsync<object>(sql);

            sql = "select lastname from students group by lastname;";
            var data3 = await DbExecuter.QueryAsync<JObject>(sql);
            var names = data3.Select(r => r.GetValue("lastname")!.ToString());

            return Ok((datas, names));
        }

        [HttpGet]
        public async Task<IActionResult> AdoNetQueryOne()
        {
            var guids = new Guid[] { new Guid("B0C1C8A4-16DD-40F2-862F-79DD0B82F037") };
            List<MySqlParameter> sqlParameters = new List<MySqlParameter>();
            StringBuilder ids = new();
            for (var i = 0; i < guids.Count(); i++)
            {
                ids.Append($"@id{i},");
                sqlParameters.Add(new MySqlParameter($"@id{i}", guids[i]));
            }
            string @id = ids.ToString().TrimEnd(',');
            string sql = $"select * from Students where id in ({@id})";
            var datas = await DbExecuter.QueryFirstOrDefaultAsync<Student>(sql, sqlParameters.ToArray());
            return Ok(datas);
        }

        [HttpGet]
        public async Task<IActionResult> AdoNetScalar()
        {
            var guids = new Guid[] { new Guid("B0C1C8A4-16DD-40F2-862F-79DD0B82F037"), new Guid("1771F732-B700-4120-8BD9-A39B4654AE72") };
            List<MySqlParameter> sqlParameters = new List<MySqlParameter>();
            StringBuilder ids = new();
            for (var i = 0; i < guids.Count(); i++)
            {
                ids.Append($"@id{i},");
                sqlParameters.Add(new MySqlParameter($"@id{i}", guids[i]));
            }
            string @id = ids.ToString().TrimEnd(',');
            string sql = $"select count(*) from Students where id in ({@id})";
            var data1 = await DbExecuter.ExecuteScalarAsync<int>(sql, sqlParameters.ToArray());

            return Ok(data1);
        }
        #endregion

        #region operationlog

        [HttpPost]
        public async Task RecordOperationAdd()
        {
            await Repo.InsertAsync(new Student
            {
                FirstMidName = "A001",
                LastName = "A001"
            });

            await Repo.InsertAsync(new Student
            {
                FirstMidName = "A002",
                LastName = "A002"
            });

            await RepoApp.InsertAsync(new AppVersion
            {
                Version = "123",
                Path = "1234"
            });

            await ClassRepo.InsertAsync(new ClassRoom
            {
                CreationUserId = 123
            });
        }

        [HttpPost]
        public async Task<string> RecordOperationLogs(string a)
        {
            await Repo.InsertAsync(new Student
            {
                FirstMidName = "C001",
                LastName = "C001"
            });

            var data = await Repo.FirstOrDefaultAsync(r => r.FirstMidName.Equals("A001"));
            data!.LastName = "A001.001";

            var data1 = await Repo.FirstOrDefaultAsync(r => r.FirstMidName.Equals("A002"));
            await Repo.DeleteAsync(data1!);

            var data2 = await RepoApp.GetAll().FirstOrDefaultAsync();
            await RepoApp.DeleteAsync(data2!);

            return "Ok123";
        }

        /// <summary>
        /// ʹ��ado.netʱ��efcore-entry����������û��ֵ�ģ����Ի�ȡ�����б����ʵ��
        /// </summary>
        /// <param name="aa"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> AdoRecordOperationLogs([FromBody] string aa) 
        {
            //DbExecuter.BeginWithEFCoreTransaction(UnitOfWorkManager.Current!);
            //using var scope = await DbExecuter.BeginTransactionAsync();

            string sql = @"insert into Students(Id,LastName,FirstMidName,EnrollmentDate,TenantId,CreationUserId,IsDeleted) " +
               "values(@Id,@LastName,@FirstMidName,@EnrollmentDate,@TenantId,@CreationUserId,@IsDeleted)";
            MySqlParameter[] sqlParameters =
            {
                new MySqlParameter("@Id",Guid.NewGuid()),
                new MySqlParameter("@LastName",8899),
                new MySqlParameter("@FirstMidName","aabb121212"),
                new MySqlParameter("@EnrollmentDate",DateTime.Now),
                new MySqlParameter("@TenantId",1),
                new MySqlParameter("@CreationUserId",1),
                new MySqlParameter("@IsDeleted",false),
            };
            var c1 = await DbExecuter.ExecuteNonQuery(sql, sqlParameters);

            string updatesql = $"update Students set LastName = @LastName where FirstMidName = @FirstMidName";
            MySqlParameter[] upsqlParameters =
            {
                 new MySqlParameter("@LastName","A001.001"),
                 new MySqlParameter("@FirstMidName","A001"),
            };
            var c2 = await DbExecuter.ExecuteNonQuery(updatesql, upsqlParameters);

            string deletesql = "delete from AppVersion where Version = @Version";
            MySqlParameter[] deleteParameters =
            {
                new MySqlParameter("@Version","123"),
            };
            var c3 = await DbExecuter.ExecuteNonQuery(deletesql, deleteParameters);

            deletesql = "delete from Students where FirstMidName = @FirstMidName";
            MySqlParameter[] deleteParameterss =
            {
                new MySqlParameter("@FirstMidName","A002")
            };
            var c4 = await DbExecuter.ExecuteNonQuery(deletesql, deleteParameterss);

            //await scope.CommitAsync();

            return (c1,c2,c3,c4);
        }

        #endregion


        #region Multiple data sources

        /// <summary>
        /// ������Դ����
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> MultipleDataSource() 
        {
            //mysql
            var data = await Repo.FirstOrDefaultAsync(r => r.LastName == "�µ�����1234");
            data!.LastName = "�µ�����1234";

            //mssql
            var data2 = await Repo1.FirstOrDefaultAsync(r => r.LastName == "Suppress1234");
            data2!.LastName = "Suppress1234";

            return Ok((data,data2));
        }

        #endregion
    }
}