using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Services;
using System;
using System.Threading;

public sealed class TurretComponent : Component
{
	[Property] GameObject Gun { get; set; }
	[Property] GameObject Muzzle { get; set; }
	[Property] ParticleEffect MuzzleParticleEmitter { get; set; }

	[Property] ModelRenderer MuzzleModel { get; set; }
	[Property] Gradient GunColorGradient { get; set; }
	[Property] Curve GunSizeCurve { get; set; }

	float turretYaw;
	float turretPitch;

	TimeSince timeSinceLastSecondary;

	TimeSince timeSincePrimary = 10;

	void FlashMuzzleModel()
	{
		if ( !MuzzleModel.IsValid() ) return;
		if ( timeSincePrimary < 0 ) return;

		MuzzleModel.Tint = GunColorGradient.Evaluate( timeSincePrimary * 2.0f );
		MuzzleModel.LocalScale = GunSizeCurve.Evaluate( timeSincePrimary * 2.0f );
	}

	protected override void OnUpdate()
	{
		var boxEmitter = MuzzleParticleEmitter.GetComponent<ParticleBoxEmitter>();

		if ( Input.Pressed( "Attack1" ) )
		{
			boxEmitter.Duration = 1;
		}

		if ( Input.Released( "Attack1" ) )
		{
			boxEmitter.Duration = 0;
		}

		FlashMuzzleModel();

		var rotationMouse = Input.Down( "Attack1" ) ? 0.05f : 0.1f;

		// rotate gun using mouse input
		turretYaw -= Input.MouseDelta.x * rotationMouse;
		turretPitch += Input.MouseDelta.y * rotationMouse;
		turretPitch = turretPitch.Clamp( -30, 30 );
		Gun.WorldRotation = Rotation.From( turretPitch, turretYaw, 0 );

		// drive tank
		Vector3 movement = 0;
		if ( Input.Down( "Forward" ) )
		{
			movement += WorldTransform.Forward;
		}

		if ( Input.Down( "backward" ) )
		{
			movement += WorldTransform.Backward;
		}

		var rot = GameObject.WorldRotation;
		var pos = GameObject.WorldPosition + movement * Time.Delta * 100.0f;

		if ( Input.Down( "Left" ) )
		{
			rot *= Rotation.From( 0, Time.Delta * 90.0f, 0 );
		}

		if ( Input.Down( "Right" ) )
		{
			rot *= Rotation.From( 0, Time.Delta * -90.0f, 0 );
		}

		LocalTransform = new Transform( pos, rot, 1 );

		RenderDebugStuff();
	}

	void RenderDebugStuff()
	{
		var bbox = BBox.FromPositionAndSize( 0, 5 );

		var tr = Scene.Trace
			.Ray( Muzzle.WorldPosition + Muzzle.WorldRotation.Forward * 50.0f, Muzzle.WorldPosition + Muzzle.WorldRotation.Forward * 4000 )
			.Size( bbox )
			//.Radius( 40 )
			.Run();

		Gizmo.Transform = global::Transform.Zero;
		Gizmo.Draw.Color = Color.White;
		Gizmo.Draw.LineThickness = 1;
		Gizmo.Draw.Line( tr.StartPosition, tr.EndPosition );
		Gizmo.Draw.Line( tr.HitPosition, tr.HitPosition + tr.Normal * 30.0f );

		Gizmo.Draw.LineSphere( new Sphere( tr.HitPosition, 10.0f ) );

		if ( tr.Body is not null )
		{
			var closestPos = tr.Body.FindClosestPoint( tr.HitPosition );
			Gizmo.Draw.LineSphere( new Sphere( closestPos, 10.0f ) );
		}

		var box = bbox;
		box.Mins += tr.EndPosition;
		box.Maxs += tr.EndPosition;

		Gizmo.Draw.LineBBox( box );
		// Gizmo.Draw.Text( $"{tr.EndPosition}", Muzzle.WorldTransform );
		// Gizmo.Draw.Text( $"{tr.HitPosition}\n{tr.Fraction}\n{tr.Direction}", new Transform( tr.HitPosition ) );

		using ( Gizmo.Scope( "circle", new Transform( tr.HitPosition, Rotation.LookAt( tr.Normal ) ) ) )
		{
			Gizmo.Draw.LineCircle( 0, 30 );
		}
	}
}
