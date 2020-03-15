// node.js script for for 2^k analysis.

/** Transpose a given matrix */
function transpose(a) {
	let result = []
	for (let i = 0; i < a[0].length; i++) {
		result[i] = []	
		for (let j = 0; j < a.length; j++) {
			result[i][j] = a[j][i]	
		}
	}
	return result;
}

/** Create powerset of size k */
function powerSet(k) {
	if (k < 2) { throw "Error, condition 2 < k <= 10 must be met."; }
	if (k > 10) { throw "Unsupported- only 2 < k <= 10"; }
	const pset = []
	const limit = Math.pow(2, k);
	
	for (let i = 0; i < limit; i++) {
		const row = []
		pset[i] = row;
		
		for (let j = 0; j < k; j++) {
			if (bitOn(i, j)) {
				row[row.length] = j	
			}
		}
	}
	
	return pset;
}

/** Comparison used to compare powerset keys into desired order, eg...
A < B < C < D < AB < AC < AD < BC < BD < CD < ABC < ABD < ACD < BCD < ABCD */
function psetCompare(a,b) { 
	if (a.length !== b.length) {
		return a.length - b.length;
	}
	for (let i = 0; i < a.length; i++) {
		const cmp = a[i] - b[i];
		if (cmp !== 0) { return cmp; }
	}
	return 0;
}

/** Create the (2^k)x(2^k) Sign Table Matrix */
function makeSignTable(k) {
	if (k < 2) { return null; }
	const mat = []
	const limit = Math.pow(2, k);
	const pset = powerSet(k);
	pset.sort(psetCompare);
	
	for (let i = 0; i < pset.length; i++) {
		const row = [ ]
		
		for (let j = 0; j < pset.length; j++) {
			const composition = pset[j];
			let val = 1;
			for (let c = 0; c < composition.length; c++) {
				val *= bitOn(i, composition[c]) ? 1 : -1;
			}
			row[j] = val;
		}
		mat[i] = row;
	}
	return mat;
}

/** Helper to print signTable matricies */
function printRows(mat) {
	console.log("[");
	for (let i = 0; i < mat.length; i++) {
		console.log(`\t${JSON.stringify(mat[i])}`);	
	}
	console.log("]");
}

/** Matrix */
// function mul(row, n) {
	// let result = [];
	// for (let i = 0; i < row.length; i++) {
		// result[i] = row[i] *= n;
	// }
	// return result
// }

/** Add two rows together */
function add(rowa, rowb) {
	let result = [];
	for (let i = 0; i < rowa.length; i++) {
		result[i] = rowa[i] + rowb[i];
	}
	return result;
}

/** Matrix multiply */
function mul(a, b) {
	let an = a.length;
	let bn = b.length;
	let cn = b[0].length;
	
	if (b.length != bn) {
		var s = `Expected matricies of sane lengths (a x b) (b x c), but have lengths (${an}x${a.length}) (${bn}x${cn})`
		throw s;
	}
	
	result = []
	for (let i = 0; i < an; i++) {
		result[i] = [];
		for (let j = 0; j < cn; j++) {
			result[i][j] = 0;	
		}
	}
	for (let i = 0; i < an; i++) {
		for (let j = 0; j < bn; j++) {
			for (let k = 0; k < cn; k++) {
				result[i][k] += a[i][j] * b[j][k]
			}
			
		}
	}
	
	return result
}


/** Helper, Vector to Matrix */
function vtom(v) {
	let result = []
	for (let i = 0; i < v.length; i++) {
		result[i] = [v[i]];	
	}
	return result;
}
/** Helper Matrix to column Vector */
function mtov(m, col) {
	if (!col) { col = 0; }
	let result = []
	for (let i = 0; i < m.length; i++) {
		result[i] = m[i][col];	
	}
	return result;
}


/** Helper, tells if 'bit' is on in  integer 'n' */
function bitOn(n, bit) { return (n & 1 << bit) != 0; }

/** Plain Ascii-code lexicographic comparison */
function strcmp(a,b) {
	for (let i = 0; i < a.length && i < b.length; i++) {
		if (a[i] < b[i]) { return -1; }
		if (a[i] > b[i]) { return 1; }
	}
	if (a.length != b.length) { return a.length - b.length; }
	return 0;
}

/** Builds names of SSTs for use in displaying data. */
function names(k) {
	/** Comparison for sorting names in SST array */
	function SSTCompare(a,b) {
		if (a === b) { return 0; }
		// Consider SST the lowest.
		if (a === "SST") { return -1; }
		if (b === "SST") { return 1; }
		
		// Consider shorter strings always lower.
		if (a.length !== b.length) {
			return a.length - b.length;	
		}
		// Otherwise, alphabetical order
		return strcmp(a,b);
	}
	// All letters but T to avoid collision with "SST"
	const LETTERS = "ABCDEFGHIJKLMNOPQRSUVWXYZ";
	const names = [ "SST" ] // T for total in index 0
	const limit = Math.pow(2,k);
	if (k > 10) { return null;	}
	
	for (let i = 1; i < limit; i++) {
		let s = "";
		for (let j = 0; j < k; j++) {
			if (bitOn(i, j)) { s += LETTERS[j]; }
		}
		names[i] = "SS"+s;
	}
	
	names.sort( SSTCompare );
	return names;
}

/** Crunch the general Sum-Squares/Total data out of the q[] coefficient vector. */
function GeneralSST(v) {
	/** Comparison to sort variations by strength of varaition. */
	function VariationCompare(a, b) {
		return a.val < b.val ? -1 
			: b.val < a.val ? 1 : 0
	}
	const k = Math.log2(v.length);
	if (k % 1 != 0) { return null; }
	
	const SS = [ 0 ]
	const ns = names(k);
	
	// Calculate 
	for (let i = 1; i < v.length; i++) {
		SS[i] = v.length * v[i] * v[i]; // 2^k * qi^2
		SS[0] += SS[i];
	}
	
	const result = {
		Column_qx: v,
		SS: {},
		Variation: []
	};
	// could interleave these, but it would produce output sorted differently.
	for (let i = 0; i < ns.length; i++) {
		result.SS[ns[i]] = SS[i];
		if (i == 0) { continue; }
		const v = SS[i] / SS[0];
		const vrn = {}
		result.Variation[i-1] = vrn
		vrn.group = ns[i].replace("SS", "");
		vrn.val = v;
	}
	result.Variation.sort(VariationCompare);
	
	return result;
}

/** Fully analyize the given data. {data} Must be a (2^k)x(n) data matrix. */
function Analyze(data) {
	/** Unscales the given matrix, dividing each element by its length. */
	function unscale(m) {
		for (let i = 0; i < m.length; i++) {
			for (let k = 0; k < m[0].length; k++) {
				m[i][k] /= m.length;
			}
		}
	}
	const k = Math.log2(data.length);
	if (k % 1 != 0) { console.log(`k=${k} must be an integer.`); return ; }
	
	let matrix = transpose(makeSignTable(k));
	if (!matrix) { console.log(`k=${k} is Currently unsupported.`); return; }
	
	// Create matrix of multiplied data (for multifactor)
	const qm = mul(matrix, data);
	// Unscale each element by the number of rows (2^k)
	unscale(qm);
	
	console.log(`====================================================================`);
	console.log(`====================================================================`);
	console.log(`\nAnalysis of 2^(k=${k}) data = `)
	console.log(data);
	// Loop over each col in the data matrix
	for (let i = 0; i < data[0].length; i++) {
		// Grab the q coefficient vector 
		const q = mtov(qm, i);
		console.log(`\n--------------------------------------`)
		//console.log(`\nColumn q[${i}] = `)
		//console.log(q);
		// Perform general SST on that vector.
		const ssts = GeneralSST(q);
		console.log(`\nSSTs for column q[${i}]:`);
		console.log(ssts);
	}
}

const fs = require("fs");
let args = []
for (let i = 2; i < process.argv.length; i++) {
	args[i-2] = process.argv[i];
}
// console.log(args);
for (let i = 0; i < args.length; i++) {
	const filename = args[i];
	try {
		const raw = fs.readFileSync(filename);
		const json = raw.toString("utf-8");
		const data = JSON.parse(json);
		console.log(`Loaded ${filename}!`);
		
		if (Array.isArray(data)) {
			Analyze(data);
		} else {
			console.log(`Whoops, ${filename} does not have a JSON array in it!`);
		}
		
	} catch (err) {
		console.log(`Could not read file ${filename}`);
		console.log(err);
	}
}