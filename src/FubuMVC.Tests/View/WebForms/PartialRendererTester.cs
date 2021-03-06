using System;
using System.Web.UI;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.View;
using FubuMVC.Core.View.Activation;
using FubuMVC.WebForms;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuMVC.Tests.View.WebForms
{
    [TestFixture]
    public class PartialRendererTester
    {
        private IWebFormsControlBuilder _builder;
        private InMemoryFubuRequest _request;

        [SetUp]
        public void SetUp()
        {
            _builder = MockRepository.GenerateMock<IWebFormsControlBuilder>();
            _request = new InMemoryFubuRequest();
        }

        [Test]
        public void should_throw_if_type_is_not_a_control()
        {
            typeof(InvalidOperationException).ShouldBeThrownBy(
                () => new PartialRenderer(_builder, null, null).CreateControl(typeof(string)));
        }

        [Test]
        public void should_throw_if_type_is_not_a_IFubuPage()
        {
            typeof(InvalidOperationException).ShouldBeThrownBy(
                () => new PartialRenderer(_builder, null, null).CreateControl(typeof(Page)));
        }


        [Test]
        public void should_create_control_from_virtual_path()
        {
            _builder.Expect(b => b.LoadControlFromVirtualPath("~/View/WebForms/TestControl.ascx", typeof(TestControl))).Return(
                new TestControl());

            new PartialRenderer(_builder, null, new InMemoryFubuRequest()).CreateControl(typeof(TestControl));

            _builder.VerifyAllExpectations();
        }

        [Test]
        public void should_execute_the_control_rendering_when_rendering()
        {
            _builder.Stub(b => b.LoadControlFromVirtualPath(null, null))
                .IgnoreArguments()
                .Return(new TestControl());

            _builder.Expect(b => b.ExecuteControl(null, null)).IgnoreArguments();

            _request.Set(new TestViewModel());

            new PartialRenderer(_builder, new StubActivator(), new InMemoryFubuRequest())
                .Render(new TestView(), typeof(TestControl), new TestControlViewModel(), "");

            _builder.VerifyAllExpectations();
        }
    }

    public class StubActivator : IPageActivator
    {
        public void Activate(IFubuPage page)
        {
        }
    }

    [TestFixture]
    public class PartialRenderer_setting_parent_page
    {
        private IWebFormsControlBuilder _builder;
        private SpecificationExtensions.CapturingConstraint _executeCatcher;
        private TestView _parentView;
        private PartialRenderer _renderer;
        private InMemoryFubuRequest _request;

        [SetUp]
        public void SetUp()
        {
            _builder = MockRepository.GenerateMock<IWebFormsControlBuilder>();
            _request = new InMemoryFubuRequest();

            _builder.Stub(b => b.LoadControlFromVirtualPath(null, null))
                   .IgnoreArguments()
                   .Return(new TestControl());

            _executeCatcher = _builder.CaptureArgumentsFor(b => b.ExecuteControl(null, null));

            _parentView = new TestView();
            _renderer = new PartialRenderer(_builder, new StubActivator(), _request);
        }

        [Test]
        public void should_set_the_parent_page()
        {
            _renderer.Render(_parentView, typeof(TestControl), new TestControlViewModel(), "");

            var control = _executeCatcher.First<Page>().Controls[0].ShouldBeOfType<TestControl>();
            ((INeedToKnowAboutParentPage)control).ParentPage.ShouldBeTheSameAs(_parentView);
        }

        [Test]
        public void should_set_the_parent_page_if_no_parent_is_specified()
        {
            _renderer.Render(new TestControl(), new TestControlViewModel(), "");

            var control = _executeCatcher.First<Page>().Controls[0].ShouldBeOfType<TestControl>();
            ((INeedToKnowAboutParentPage)control).ParentPage.ShouldBeOfType<Page>();
        }
    }

    public class TestView : FubuPage<TestViewModel>
    {
    }

    public class TestControlViewModel
    {
        public string Name { get; set; }
    }

    public class TestControl : FubuControl<TestControlViewModel>
    {
    }
}