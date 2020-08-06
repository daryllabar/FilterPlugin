using Source.DLaB.Common;
using System;

namespace DLaB.Xrm.FilterPlugin.Plugin
{
    public abstract class ConfigDataPlugin<TConfig> : GenericPluginBase<ExtendedPluginContext>
        where TConfig : class, new()
    {
        public TConfig SecureConfigData { get; set; }
        public TConfig UnsecureConfigData { get; set; }
        public TConfig ConfigData => SecureConfigData ?? UnsecureConfigData ?? Default ?? (Default = CreateDefaultConfigData());
        private TConfig Default { get; set; }

        public Exception ConfigParseError { get; set; }

        /// <inheritdoc />
        protected ConfigDataPlugin(string unsecureConfig, string secureConfig) : base(unsecureConfig, secureConfig)
        {
            UnsecureConfigData = Parse<TConfig>(unsecureConfig);
            SecureConfigData = Parse<TConfig>(secureConfig);
        }

        private T Parse<T>(string config)
            where T: new()
        {
            if (ConfigParseError != null)
            {
                return default;
            }

            try
            {
                return string.IsNullOrWhiteSpace(config)
                    ? default
                    : config.DeserializeJson<T>();
            }
            catch (Exception ex)
            {
                ConfigParseError = ex;
            }

            return default;
        }

        public virtual TConfig CreateDefaultConfigData()
        {
            return new TConfig();
        }

        protected override bool SkipExecution(ExtendedPluginContext context)
        {
            if (ConfigParseError == null)
            {
                return base.SkipExecution(context);
            }
            context.Trace("An exception occured attempting to deserialize the config data:");
            context.LogException(ConfigParseError);
            return true;
        }

        /// <inheritdoc />
        protected override ExtendedPluginContext CreatePluginContext(IServiceProvider serviceProvider)
        {
            return new ExtendedPluginContext(serviceProvider, this);
        }
    }
}
