using System;
using System.IO;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace AspnetCore.DI.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceOptionsDemo2();
        }

        private static void AutofacDemo()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<MyService>();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<MyService>();
            containerBuilder.Populate(services);

            var container = containerBuilder.Build();
            var serviceProvider = new AutofacServiceProvider(container);
            var myService = serviceProvider.GetService<MyService>();
        }

        private static void CastleWindsorDemo()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<MyService>();

            var windsorContainer = new WindsorContainer();

            windsorContainer.Register(
                Component.For<MyService>()
            );

            var serviceProvider = WindsorRegistrationHelper.CreateServiceProvider(
                windsorContainer,
                services
            );

            var myService = serviceProvider.GetService<MyService>();
        }

        private static void ServiceOptionsDemo2()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
                .Build();

            IServiceCollection services = new ServiceCollection();

            services.AddOptions();
            services.AddScoped<MyTaxCalculator>();

            services.Configure<MyTaxCalculatorOptions>(configuration.GetSection("TaxOptions"));

            var serviceProvider = services.BuildServiceProvider();

            var calculator = serviceProvider.GetRequiredService<MyTaxCalculator>();
            Console.WriteLine(calculator.Calculate(200));
        }

        private static void ServiceOptionsDemo1()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddOptions();
            services.AddTransient<MyTaxCalculator>();
            services.Configure<MyTaxCalculatorOptions>(options =>
            {
                options.TaxRatio = 135;
            });

            var serviceProvider = services.BuildServiceProvider();
            var calculator = serviceProvider.GetService<MyTaxCalculator>();

            Console.WriteLine(calculator.Calculate(100));
        }

        private static void MultipleImplementation()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<IHasValue, MyClassWithValue>();
            services.AddTransient<IHasValue, MyClassWithValue2>();

            var serviceProvider = services.BuildServiceProvider();
            var myServices = serviceProvider.GetServices<IHasValue>().ToList();
            var myService = serviceProvider.GetService<IHasValue>();

            Console.WriteLine("----- Multiple Implemantation Services -----------");

            foreach (var service in myServices)
            {
                Console.WriteLine(service.Value); 
            }

            Console.WriteLine("----- Multiple Implemantation Service ------------");
            Console.WriteLine(myService.Value);
        }

        private static void MultipleImplementationWithTry()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<IHasValue, MyClassWithValue>();
            services.TryAddTransient<IHasValue, MyClassWithValue2>();

            var serviceProvider = services.BuildServiceProvider();
            var myServices = serviceProvider.GetServices<IHasValue>().ToList();

            Console.WriteLine("----- Multiple Implemantation Try ----------------");

            foreach (var service in myServices)
            {
                Console.WriteLine(service.Value);
            }
        }

        private static void MultipleImplementationWithReplace()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<IHasValue, MyClassWithValue>();
            services.Replace(ServiceDescriptor.Transient<IHasValue, MyClassWithValue2>());

            var serviceProvider = services.BuildServiceProvider();
            var myServices = serviceProvider.GetServices<IHasValue>().ToList();

            Console.WriteLine("----- Multiple Implemantation Replace ------------");

            foreach (var service in myServices)
            {
                Console.WriteLine(service.Value);
            }

            Console.WriteLine("--------------------------------------------------");
        }

        private static void InstanceRegistrationDemo()
        {
            var instance = new MyInstance { Value = 44 };

            IServiceCollection services = new ServiceCollection();

            services.AddSingleton(instance);

            foreach (ServiceDescriptor service in services)
            {
                if (service.ServiceType == typeof(MyInstance))
                {
                    var registeredInstance = (MyInstance)service.ImplementationInstance;

                    Console.WriteLine("Registered instance : " + registeredInstance.Value);
                }
            }

            var serviceProvider = services.BuildServiceProvider();
            var myInstance = serviceProvider.GetService<MyInstance>();

            Console.WriteLine("Registered service by instance registration : " + myInstance.Value);
        }

        private static void FactoryMethodDemo()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<IMyServiceDependency, MyServiceDependency>();
            // Overload method for factory registration
            services.AddTransient(
                provider => new MyService(provider.GetService<IMyServiceDependency>())
            );

            var serviceProvider = services.BuildServiceProvider();
            var instance = serviceProvider.GetService<MyService>();

            instance.DoIt();
        }

        private static void Demo2()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<TransientDateOperation>();
            services.AddScoped<ScopedDateOperation>();
            services.AddSingleton<SingletonDateOperation>();

            var serviceProvider = services.BuildServiceProvider();

            Console.WriteLine();
            Console.WriteLine("-------- 1st Request --------");
            Console.WriteLine();

            var transientService = serviceProvider.GetService<TransientDateOperation>();
            var scopedService = serviceProvider.GetService<ScopedDateOperation>();
            var singletonService = serviceProvider.GetService<SingletonDateOperation>();

            Console.WriteLine();
            Console.WriteLine("-------- 2nd Request --------");
            Console.WriteLine();

            var transientService2 = serviceProvider.GetService<TransientDateOperation>();
            var scopedService2 = serviceProvider.GetService<ScopedDateOperation>();
            var singletonService2 = serviceProvider.GetService<SingletonDateOperation>();

            Console.WriteLine();
            Console.WriteLine("-----------------------------");
            Console.WriteLine();
        }

        private static void Demo1()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<MyService>();

            var serviceProvider = services.BuildServiceProvider();
            var myService = serviceProvider.GetService<MyService>();

            myService.DoIt();
        }

        private static void Demo3()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddTransient<TransientDateOperation>();
            services.AddScoped<ScopedDateOperation>();
            services.AddSingleton<SingletonDateOperation>();

            var serviceProvider = services.BuildServiceProvider();

            Console.WriteLine();
            Console.WriteLine("-------- 1st Request --------");
            Console.WriteLine();

            using (var scope = serviceProvider.CreateScope())
            {
                var transientService = scope.ServiceProvider.GetService<TransientDateOperation>();
                var scopedService = scope.ServiceProvider.GetService<ScopedDateOperation>();
                var singletonService = scope.ServiceProvider.GetService<SingletonDateOperation>();
            }

            Console.WriteLine();
            Console.WriteLine("-------- 2nd Request --------");
            Console.WriteLine();

            using (var scope = serviceProvider.CreateScope())
            {
                var transientService = scope.ServiceProvider.GetService<TransientDateOperation>();
                var scopedService = scope.ServiceProvider.GetService<ScopedDateOperation>();
                var singletonService = scope.ServiceProvider.GetService<SingletonDateOperation>();
            }

            Console.WriteLine();
            Console.WriteLine("-----------------------------");
            Console.WriteLine();
        }
    }

    public class MyTaxCalculator
    {
        private readonly MyTaxCalculatorOptions _options;

        public MyTaxCalculator(IOptions<MyTaxCalculatorOptions> options)
        {
            _options = options.Value;
        }

        public int Calculate(int amount)
        {
            return amount * _options.TaxRatio / 100;
        }
    }

    public class MyTaxCalculatorOptions
    {
        public int TaxRatio { get; set; }

        public MyTaxCalculatorOptions()
        {
            TaxRatio = 118;
        }
    }

    public interface IHasValue
    {
        object Value { get; set; }
    }

    public class MyClassWithValue : IHasValue
    {
        public object Value { get; set; }

        public MyClassWithValue()
        {
            Value = 44;
        }
    }

    public class MyClassWithValue2 : IHasValue
    {
        public object Value { get; set; }

        public MyClassWithValue2()
        {
            Value = 4444;
        }
    }

    public class MyInstance
    {
        public int Value { get; set; }
    }

    public class MyService
    {
        private readonly IMyServiceDependency _dependency;

        public MyService(IMyServiceDependency dependency)
        {
            _dependency = dependency;
        }

        public void DoIt()
        {
            _dependency.DoIt();
        }

        public DateTime GetDate()
        {
            return DateTime.Now;
        }
    }

    public class MyServiceDependency : IMyServiceDependency
    {
        public void DoIt()
        {
            Console.WriteLine("Hello from MyServiceDependency");
        }
    }

    public interface IMyServiceDependency
    {
        void DoIt();
    }

    public class TransientDateOperation
    {
        public TransientDateOperation()
        {
            Console.WriteLine("Transient service is created!");
        }
    }

    public class ScopedDateOperation
    {
        public ScopedDateOperation()
        {
            Console.WriteLine("Scoped service is created!");
        }
    }

    public class SingletonDateOperation
    {
        public SingletonDateOperation()
        {
            Console.WriteLine("Singleton service is created!");
        }
    }
}