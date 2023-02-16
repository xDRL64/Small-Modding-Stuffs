let getFloat32_from2Bytes = (A,B)=>{
	//let UI8A = new Uint8Array([0,0,A,B]);
	//let UI8A = new Uint8Array([B,A,0,0]);
	//let UI8A = new Uint8Array([A,B,0,0]);
	//return (new Float32Array(UI8A.buffer))[0];

//	//let UI8A = new Uint8Array([A,B]);
//	let UI8A = new Uint8Array([B,A]);
//	return (new Float16Array(UI8A.buffer))[0];

//	//let UI8A = new Uint8Array([B,A]);
//	let UI8A = new Uint8Array([A,B]);
//	return (new Int16Array(UI8A.buffer))[0];

	return (A<<8)+B;
//	return (B<<8)+A;
};

let get_vertices = (binFile, vertStartAddress, vertOffset, vertByteSize)=>{

	let address = vertStartAddress+vertOffset;

	let data = binFile.slice(address, address+vertByteSize);

	let vertCount = vertByteSize >> 4; // div by 16

	let vertices = [];

	for(let iVert=0; iVert<vertCount; iVert++){
		let iVertOfst = iVert << 4; // mul by 16
		let vertex = {x:0,y:0,z:0, u:0,v:0, r:0,g:0,b:0,a:0,};

		vertex.x = getFloat32_from2Bytes(data[iVertOfst  ], data[iVertOfst+1]);
		vertex.y = getFloat32_from2Bytes(data[iVertOfst+2], data[iVertOfst+3]);
		vertex.z = getFloat32_from2Bytes(data[iVertOfst+4], data[iVertOfst+5]);

		vertex.u = getFloat32_from2Bytes(data[iVertOfst+8 ], data[iVertOfst+9 ]);
		vertex.v = getFloat32_from2Bytes(data[iVertOfst+10], data[iVertOfst+11]);

		vertex.r = data[iVertOfst+12] / 255;
		vertex.g = data[iVertOfst+13] / 255;
		vertex.b = data[iVertOfst+14] / 255;
		vertex.a = data[iVertOfst+15] / 255;

		vertices.push(vertex);
	}

	return vertices;
};

let getDirectGeo_fromVerticesObj = (vObj)=>{
	return {
		position : vObj.map(e=>[e.x,e.y,e.z]).flat(),
		texcoord : vObj.map(e=>[e.u,e.v]).flat(),
		color    : vObj.map(e=>[e.r,e.g,e.b,e.a]).flat(),
	}
};

let compute_medianPoint = (verticesObj)=>{
	let xyzSum = verticesObj.reduce((acc,cur)=>[acc[0]+cur.x,acc[1]+cur.y,acc[2]+cur.z],[0,0,0]);
	return xyzSum.map(e=>e/verticesObj.length);
};

let get_2pointsLength = (pA, pB)=>{
	let A = vec3.create(); vec3.set(A, ...pA);
	let B = vec3.create(); vec3.set(B, ...pB);
	let C = vec3.create(); vec3.sub(C, A,B);
	return vec3.length(C);
};

let get_farestPointInfo = (verticesObj, center)=>{
	return verticesObj.reduce((acc,cur,i)=>{
		let dist = get_2pointsLength([cur.x,cur.y,cur.z],center);
		let longest = Math.max(acc.dist, dist);
		return {
			dist : longest,
			index: (acc.dist!==longest ? i : acc.index),
		};
	},{dist:-1,index:null});

}