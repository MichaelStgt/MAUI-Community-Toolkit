﻿using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.UnitTests.Mocks;

namespace CommunityToolkit.Maui.UnitTests;

public abstract class BaseHandlerTest : BaseTest
{
	protected BaseHandlerTest()
	{
		InitializeServicesAndSetMockApplication(out var serviceProvider);
		ServiceProvider = serviceProvider;
	}

	protected IServiceProvider ServiceProvider { get; }

	protected static TElementHandler CreateElementHandler<TElementHandler>(IElement view, bool doesRequireMauiContext = true)
		where TElementHandler : IElementHandler, new()
	{
		var mockElementHandler = new TElementHandler();
		mockElementHandler.SetVirtualView(view);

		if (doesRequireMauiContext)
		{
			mockElementHandler.SetMauiContext(Application.Current?.Handler?.MauiContext ?? throw new NullReferenceException());
		}

		return mockElementHandler;
	}

	protected static TViewHandler CreateViewHandler<TViewHandler>(IView view, bool doesRequireMauiContext = true)
		where TViewHandler : IViewHandler, new()
	{
		var mockViewHandler = new TViewHandler();
		mockViewHandler.SetVirtualView(view);

		if (doesRequireMauiContext)
		{
			mockViewHandler.SetMauiContext(Application.Current?.Handler?.MauiContext ?? throw new NullReferenceException());
		}

		return mockViewHandler;
	}

	static void InitializeServicesAndSetMockApplication(out IServiceProvider serviceProvider)
	{
		var appBuilder = MauiApp.CreateBuilder()
			.UseMauiCommunityToolkit()
			.UseMauiApp<MockApplication>();

		#region Register Services for CameraTests

		appBuilder.Services.AddSingleton<ICameraProvider, MockCameraProvider>();

		#endregion

		#region Register Services for PopupServiceTests

		var mockPageViewModel = new MockPageViewModel();
		var mockPopup = new MockSelfClosingPopup(mockPageViewModel, new());

		PopupService.ClearViewModelToViewMappings();
		PopupService.AddTransientPopup(mockPopup, mockPageViewModel, appBuilder.Services);
		var page = new ContentPage();
		#endregion

		var mauiApp = appBuilder.Build();

		var application = (MockApplication)mauiApp.Services.GetRequiredService<IApplication>();
		application.AddWindow(new Window() { Page = page });
		serviceProvider = mauiApp.Services;

		IPlatformApplication.Current = application;

		application.Handler = new ApplicationHandlerStub();
		application.Handler.SetMauiContext(new HandlersContextStub(serviceProvider));

		CreateElementHandler<MockPopupHandler>(mockPopup);
		CreateViewHandler<MockPageHandler>(page);
	}
}