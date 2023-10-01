shader_type spatial;

uniform sampler2D sprite;
uniform float viewing_angle = 0.5;


void fragment() {
	vec2 cam = normalize(VIEW.xz);
	vec2 norm = normalize(NORMAL.xz);
	if (dot(cam, norm) < viewing_angle) {
		discard; // viewing angle discard
	}
	vec4 sample = texture(sprite, UV);
	if (sample.a < 0.5) {
		discard; // alpha cut discard
	}
	ALBEDO = sample.rgb;
}
