import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { JSDOM } from 'jsdom';
import { RaiBridge } from '@dr2rai/raid-canvas';

const svgPath = process.argv[2];
if (!svgPath) {
  throw new Error('Usage: node scripts/verify-raid-canvas-hydration.mjs <diagram.svg>');
}

const svg = await readFile(svgPath, 'utf8');
const jsdom = new JSDOM();
globalThis.DOMParser = jsdom.window.DOMParser;
globalThis.XMLSerializer = jsdom.window.XMLSerializer;

const addedNodes = [];
const addedEdges = [];
const graph = {
  clearCells() {},
  addNode(node) {
    addedNodes.push(node);
    return { setZIndex() {}, addChild() {} };
  },
  addEdge(edge) {
    addedEdges.push(edge);
    return edge;
  },
  getCellById() { return undefined; },
  on() {},
  getPorts() { return []; },
  getNodes() { return []; },
  getEdges() { return []; },
};

const model = new RaiBridge().hydrateFromSvg(svg, graph);
assert.ok(model.nodes.length > 0, 'RaidCanvas must hydrate at least one node');
assert.ok(model.edges.length > 0, 'RaidCanvas must hydrate at least one edge');
assert.equal(addedNodes.length, model.nodes.length);
assert.equal(addedEdges.length, model.edges.length);
for (const node of model.nodes) {
  for (const coordinate of [node.bounds.x, node.bounds.y, node.bounds.width, node.bounds.height]) {
    assert.ok(Number.isFinite(coordinate), `Non-finite node geometry for ${node.id}`);
  }
}
console.log(`hydrated ${model.nodes.length} nodes and ${model.edges.length} edges`);
