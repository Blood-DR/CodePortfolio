using Sandbox;

public sealed class CctvDisplay : Component
{

	/// setup render targets for camera to display
	/// simulates cctv camera's rendering to a computer screen


	[Property, Group( "Camera" )] public CameraComponent DisplayCamera1 { get; set; }
	[Property, Group( "Camera" )] public CameraComponent DisplayCamera2 { get; set; }
	[Property, Group( "CameraNV" )] public CameraComponent DisplayCamera3 { get; set; }

	[Property, Group( "Displays" )] public GameObject CameraPlaneTL { get; set; }
	[Property, Group( "Displays" )] public GameObject CameraPlaneTR { get; set; }
	[Property, Group( "Displays" )] public GameObject CameraPlaneBL { get; set; }
	[Property, Group( "Displays" )] public GameObject CameraPlaneBR { get; set; }
	[Property, Group( "Display" )] public Material MonitorBaseMaterial { get; set; }
	[Property, Group( "Display" )] public Material MonitorNVMaterial { get; set; }
	protected override void OnStart()
	{

		if ( DisplayCamera1 == null )
		{
			Log.Warning( "[CctvDisplay] DisplayCamera1 not assigned!" );
			return;
		}
		if ( DisplayCamera2 == null )
		{
			Log.Warning( "[CctvDisplay] DisplayCamera2 not assigned!" );
			return;
		}
		if ( DisplayCamera3 == null )
		{
			Log.Warning( "[CctvDisplay] DisplayCamera3 not assigned!" );
			return;
		}

		var renderTarget1 = Texture.CreateRenderTarget()
			.WithSize( 1920, 1080 )
			.Create();

		var renderTarget2 = Texture.CreateRenderTarget()
			.WithSize( 1920, 1080 )
			.Create();

		var renderTarget3 = Texture.CreateRenderTarget()
			.WithSize( 1920, 1080 )
			.Create();


		DisplayCamera1.RenderTarget = renderTarget1;
		DisplayCamera2.RenderTarget = renderTarget2;
		DisplayCamera3.RenderTarget = renderTarget3;



		var mat1 = MonitorBaseMaterial.CreateCopy();
		mat1.Set( "Color", renderTarget1 );
		mat1.Set( "SelfIllumMask", renderTarget1 );    // same texture as emission mask
		mat1.Set( "SelfIllumScale", 2.0f );


		var mat2 = MonitorBaseMaterial.CreateCopy();
		mat2.Set( "Color", renderTarget2 );
		mat2.Set( "SelfIllumMask", renderTarget2 );    // same texture as emission mask
		mat2.Set( "SelfIllumScale", 2.0f );


		var mat3 = MonitorNVMaterial.CreateCopy();
		mat3.Set( "Color", renderTarget3 );
		mat3.Set( "SelfIllumMask", renderTarget3 );    // same texture as emission mask
		mat3.Set( "SelfIllumScale", 2.0f );

		///camera4 was not set literally, was going to be a static screen or along those lines
		
		CameraPlaneBR.GetComponent<ModelRenderer>().MaterialOverride = mat1;
		CameraPlaneTR.GetComponent<ModelRenderer>().MaterialOverride = mat2;
		CameraPlaneTL.GetComponent<ModelRenderer>().MaterialOverride = mat3;
		//CameraPlane4.GetComponent<ModelRenderer>().MaterialOverride = mat;

	}

	protected override void OnUpdate()
	{

		//the way s&box handles the material and render targets. its not necessary to update per frame, as its handled automatically


	}
}
