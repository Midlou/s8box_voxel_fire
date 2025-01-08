using System;
using Sandbox;

public sealed class GunEmitterParticleController : ParticleController
{
	[Property] GameObject Muzzle { get; set; }

	protected override void OnParticleStep( Particle particle, float delta )
	{
		if ( particle.Age < 0.15f && particle.Age > 0.10f )
		{
			particle.Velocity = Muzzle.WorldRotation.Forward * 400;
		}

		var chanceTest = Random.Shared.Next( 1, 100 ) == 1;
		if ( particle.Age > 0.20f && chanceTest )
		{
			particle.Velocity += Vector3.Up * 50;
		}
	}
}
