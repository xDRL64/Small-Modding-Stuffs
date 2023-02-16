
let build_shader = (gl)=>{

	const vertexShader = `
		attribute vec4 aVertexPosition;
		attribute vec4 aVertexColor;
		uniform mat4 uModelViewMatrix;
		uniform mat4 uProjectionMatrix;
		varying lowp vec4 vColor;
		void main(void) {
			vec4 c = aVertexColor;
			gl_Position = uProjectionMatrix * uModelViewMatrix * aVertexPosition;
			//gl_Position = aVertexPosition;
			vColor = aVertexColor;
			float v = 1.0;
		//	vColor = vec4(1.-c.r*v, 1.-c.g*v, 1.-c.b*v, 1);
		}
  	`;
	const vShader = gl.createShader(gl.VERTEX_SHADER);
	gl.shaderSource(vShader, vertexShader);
	gl.compileShader(vShader);

  	const fragmentShader = `
		varying lowp vec4 vColor;
		void main(void) {
			
			gl_FragColor = vColor;
			//gl_FragColor = vec4(1,0,0,1);
			//gl_FragColor = vec4(1.-c.r,1.-c.g,1.-c.b, 1);
		}
 	`;
	const fShader = gl.createShader(gl.FRAGMENT_SHADER);
	gl.shaderSource(fShader, fragmentShader);
	gl.compileShader(fShader);

	const shaderProgram = gl.createProgram();
	gl.attachShader(shaderProgram, vShader);
	gl.attachShader(shaderProgram, fShader);
	gl.linkProgram(shaderProgram);

	return {
		shaderProgram,
		attributes : {
			vPosition : gl.getAttribLocation(shaderProgram, "aVertexPosition"),
			vColor    : gl.getAttribLocation(shaderProgram, "aVertexColor"),
		},
		uniforms : {
			mProj : gl.getUniformLocation(shaderProgram, "uProjectionMatrix"),
			mMV   : gl.getUniformLocation(shaderProgram, "uModelViewMatrix"),
		},
	};
};