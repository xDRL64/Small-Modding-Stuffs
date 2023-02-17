let get_webglHandle = (htmlCanvas)=>{
	const gl = htmlCanvas.getContext("webgl");
	gl.enable(gl.CULL_FACE);
	gl.enable(gl.DEPTH_TEST);
	gl.depthFunc(gl.LEQUAL);

	gl.clearColor(0.0, 0.0, 0.0, 1.0);
	gl.clear(gl.COLOR_BUFFER_BIT);
	return gl;
};

let _vertexCount = 0;

let gl_bufferPosition = null;
let send_attrPosition = (gl, positions)=>{
	_vertexCount = Math.floor(positions.length/3);
	gl_bufferPosition = gl.createBuffer();
	gl.bindBuffer(gl.ARRAY_BUFFER, gl_bufferPosition);
	gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(positions), gl.STATIC_DRAW);
};

let gl_bufferColor = null;
let send_attrColor = (gl, colors)=>{
	gl_bufferColor = gl.createBuffer();
	gl.bindBuffer(gl.ARRAY_BUFFER, gl_bufferColor);
	gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(colors), gl.STATIC_DRAW);
};


let update_glAttributes = (gl, shaderStruct)=>{
	// make attributes 'positions' ready
	gl.bindBuffer(gl.ARRAY_BUFFER, gl_bufferPosition);
	position = shaderStruct.attributes.vPosition;
	gl.vertexAttribPointer(position, 3, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(position);
	// make attributes 'colors' ready
	gl.bindBuffer(gl.ARRAY_BUFFER, gl_bufferColor);
	color = shaderStruct.attributes.vColor;
	gl.vertexAttribPointer(color, 4, gl.FLOAT, false, 0, 0);
	gl.enableVertexAttribArray(color);
};

let draw_scene = (gl, shaderStruct, cam, target)=>{

	// make Projection matrix
	const fieldOfView = (60 * Math.PI) / 180; // in radians
	const aspect = gl.canvas.clientWidth / gl.canvas.clientHeight;
	const projectionMatrix = mat4.create();
	mat4.perspective(projectionMatrix, fieldOfView, aspect, 0.01, 1000000.0);
  
	// make Model View matrix
	const modelViewMatrix = mat4.create();
	let scale = 1.0001;
	mat4.scale(modelViewMatrix, modelViewMatrix, [scale,scale,scale]);
	//mat4.translate(modelViewMatrix, modelViewMatrix, cam.map(e=>-1*e));
	mat4.lookAt(modelViewMatrix, cam, target, [0,1,0]);

	// set shader
	gl.useProgram(shaderStruct.shaderProgram);

	// Set the shader uniforms
	gl.uniformMatrix4fv(shaderStruct.uniforms.mProj, false, projectionMatrix );
	gl.uniformMatrix4fv(shaderStruct.uniforms.mMV, false, modelViewMatrix );

	{
		gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

		const offset = 0;
		gl.drawArrays(gl.TRIANGLES, offset, _vertexCount);
	}
};