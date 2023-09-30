shader_type spatial;

uniform sampler2D sprite;
uniform float viewing_angle = 0.5;


void fragment() {
	vec2 cam = VIEW.xz;
	vec2 norm = NORMAL.xz;
	float amount = dot(cam, norm);
	if (amount < viewing_angle) {
		discard;
	} else {
		vec4 sample = texture(sprite, UV);
		if (sample.a < 0.5) {
			discard;
		}
		ALBEDO = sample.rgb;
	}
}
