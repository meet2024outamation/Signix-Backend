using Fluid;

namespace SharedKernal.Adapters
{
    public interface ILiquidTemplate
    {
        ValueTask<string> RenderAsync(object model);
    }

    public class FluidTemplateAdapter : ILiquidTemplate
    {
        private readonly IFluidTemplate _fluidTemplate;
        public FluidTemplateAdapter(IFluidTemplate fluidTemplate)
        {
            _fluidTemplate = fluidTemplate;
        }
        public ValueTask<string> RenderAsync(object model)
        {

            var options = new TemplateOptions();
            options.MemberAccessStrategy.Register(model.GetType());
            var context = new TemplateContext(model, options);
            return _fluidTemplate.RenderAsync(context);
        }
    }
}
